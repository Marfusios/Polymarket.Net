using Polymarket.Net.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Polymarket.Net.Signing
{
    internal static class LightEip712TypedDataEncoder
    {
        private static readonly Regex BytesPattern = new Regex(@"^bytes\d+$", RegexOptions.Compiled);
        private static readonly Regex UintPattern = new Regex(@"^uint\d+$", RegexOptions.Compiled);
        private static readonly Regex IntPattern = new Regex(@"^int\d+$", RegexOptions.Compiled);

        // Cache domain separator hashes — domain values are constant per (chainId, contract).
        private static readonly Dictionary<string, byte[]> _domainHashCache = new Dictionary<string, byte[]>();
        private static readonly object _domainHashCacheLock = new object();

        internal static byte[] EncodeTypedDataRaw(TypedDataRaw typedData)
        {
            // Fixed size: 2 bytes prefix + 32 bytes domain hash + 32 bytes message hash = 66 bytes
            var result = new byte[66];
            result[0] = 0x19;
            result[1] = 0x01;

            // Domain hash — cacheable when domain values are constant (e.g., per chainId+contract)
            var domainHash = HashStructWithDomainCache(typedData.Types, typedData.DomainRawValues);
            Buffer.BlockCopy(domainHash, 0, result, 2, 32);

            // Message hash — varies per order
            var messageHash = HashStruct(typedData.Types, typedData.PrimaryType, typedData.Message);
            Buffer.BlockCopy(messageHash, 0, result, 34, 32);

            return result;
        }

        private static byte[] HashStructWithDomainCache(IDictionary<string, MemberDescription[]> types, IEnumerable<MemberValue> domainValues)
        {
            // Build cache key from domain values (contract address is the distinguishing factor)
            var domainArray = domainValues as MemberValue[] ?? domainValues.ToArray();
            string? cacheKey = null;
            foreach (var mv in domainArray)
            {
                if (mv.TypeName == "address")
                {
                    cacheKey = (string)mv.Value;
                    break;
                }
            }

            if (cacheKey != null)
            {
                byte[]? cached;
                lock (_domainHashCacheLock)
                    _domainHashCache.TryGetValue(cacheKey, out cached);
                if (cached != null)
                    return cached;

                var hash = HashStruct(types, "EIP712Domain", domainArray);
                lock (_domainHashCacheLock)
                    _domainHashCache[cacheKey] = hash;
                return hash;
            }

            return HashStruct(types, "EIP712Domain", domainArray);
        }

        private static byte[] HashStruct(IDictionary<string, MemberDescription[]> types, string primaryType, IEnumerable<MemberValue> message)
        {
            // Pre-size: type_hash(32) + N fields × 32 bytes. Order has 12 fields = 416 bytes.
            var memoryStream = new MemoryStream(448);
            var writer = new BinaryWriter(memoryStream);

            // Encode the type header
            EncodeType(writer, types, primaryType);

            // Encode the data
            EncodeData(writer, types, message);

            writer.Flush();
            // Use GetBuffer + length to avoid ToArray() copy when possible
            return InternalSha3Keccack.CalculateHash(memoryStream.GetBuffer(), (int)memoryStream.Length);
        }

        private static void EncodeData(BinaryWriter writer, IDictionary<string, MemberDescription[]> types, IEnumerable<MemberValue> memberValues)
        {
            foreach (var memberValue in memberValues)
            {
                switch (memberValue.TypeName)
                {
                    case var refType when IsReferenceType(refType):
                        writer.Write(HashStruct(types, memberValue.TypeName, (IEnumerable<MemberValue>)memberValue.Value));
                        break;

                    case "string":
                        writer.Write(AbiEncoder.AbiValueEncodeString((string)memberValue.Value));
                        break;

                    case "bool":
                        writer.Write(AbiEncoder.AbiValueEncodeBool((bool)memberValue.Value));
                        break;

                    case "address":
                        writer.Write(AbiEncoder.AbiValueEncodeAddress((string)memberValue.Value));
                        break;

                    default:
                        if (memberValue.TypeName.Contains("["))
                        {
                            var items = (IList)memberValue.Value;
                            var itemsMemberValues = new List<MemberValue>();
                            foreach (var item in items)
                            {
                                itemsMemberValues.Add(new MemberValue()
                                {
                                    TypeName = memberValue.TypeName.Substring(0, memberValue.TypeName.LastIndexOf("[")),
                                    Value = item
                                });
                            }

                            var memoryStream = new MemoryStream();
                            var writerItem = new BinaryWriter(memoryStream);

                            EncodeData(writerItem, types, itemsMemberValues);
                            writerItem.Flush();
                            writer.Write(InternalSha3Keccack.CalculateHash(memoryStream.ToArray()));
                        }
                        else if (memberValue.TypeName.StartsWith("int") || memberValue.TypeName.StartsWith("uint"))
                        {
                            if (memberValue.Value is string v)
                            {
                                // Fast path: most order amounts fit in ulong, avoid BigInteger allocation
                                bool signed = memberValue.TypeName[0] != 'u';
                                if (!signed && ulong.TryParse(v, out ulong ulVal))
                                {
                                    writer.Write(AbiEncoder.AbiValueEncodeInt(ulVal));
                                }
                                else if (BigInteger.TryParse(v, out BigInteger parsedOutput))
                                {
                                    writer.Write(AbiEncoder.AbiValueEncodeBigInteger(signed, parsedOutput));
                                }
                                else
                                {
                                    throw new FormatException(
                                        $"Failed to parse EIP-712 integer value. solidityType={memberValue.TypeName}, valueType={memberValue.Value?.GetType().FullName ?? "null"}, value={FormatMemberValue(memberValue.Value)}");
                                }
                            }
                            else if (memberValue.Value is byte b)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(b));
                            }
                            else if (memberValue.Value is short s)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(s));
                            }
                            else if (memberValue.Value is int i)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(i));
                            }
                            else if (memberValue.Value is long l)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(l));
                            }
                            else if (memberValue.Value is ushort us)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(us));
                            }
                            else if (memberValue.Value is uint ui)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(ui));
                            }
                            else if (memberValue.Value is ulong ul)
                            {
                                writer.Write(AbiEncoder.AbiValueEncodeInt(ul));
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Unsupported EIP-712 integer CLR type. solidityType={memberValue.TypeName}, valueType={memberValue.Value?.GetType().FullName ?? "null"}, value={FormatMemberValue(memberValue.Value)}");
                            }
                        }
                        else if (memberValue.TypeName.StartsWith("bytes"))
                        {
                            // Applicable?
                            //if (memberValue.Value is string v)
                            //    writer.Write(AbiEncoder.AbiValueEncodeHexBytes(v));
                            //else if (memberValue.Value is byte[] b)
                            //    writer.Write(AbiEncoder.AbiValueEncodeBytes(b));
                            //else
                            //    throw new Exception("Unknown byte value type");

                            var length = memberValue.TypeName.Length == 5 ? 32 : int.Parse(memberValue.TypeName.Substring(5));
                            writer.Write(AbiEncoder.AbiValueEncodeBytes(length, (byte[])memberValue.Value));
                        }
                        break;

                }
            }
        }

        // Cache type hashes — the hash of a type schema string is deterministic for a given type definition.
        private static readonly Dictionary<string, byte[]> _typeHashCache = new Dictionary<string, byte[]>();
        private static readonly object _typeHashCacheLock = new object();

        private static void EncodeType(BinaryWriter writer, IDictionary<string, MemberDescription[]> types, string typeName)
        {
            // Build a cache key from the type name + member definitions (stable for same schema)
            byte[]? cached;
            lock (_typeHashCacheLock)
                _typeHashCache.TryGetValue(typeName, out cached);

            if (cached == null)
            {
                var encodedTypes = EncodeTypes(types, typeName);
                var encodedPrimaryType = encodedTypes.Single(x => x.Key == typeName);
                var encodedReferenceTypes = encodedTypes.Where(x => x.Key != typeName).OrderBy(x => x.Key).Select(x => x.Value);
                var fullyEncodedType = encodedPrimaryType.Value + string.Join(string.Empty, encodedReferenceTypes.ToArray());
                cached = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(fullyEncodedType));
                lock (_typeHashCacheLock)
                    _typeHashCache[typeName] = cached;
            }

            writer.Write(cached);
        }

        /// <summary>
        /// Create a list of type => type(parameters), for example:<br />
        /// { IP712Domain, EIP712Domain(string name,string version,uint256 chainId,address verifyingContract) }
        /// </summary>
        private static IList<KeyValuePair<string, string>> EncodeTypes(IDictionary<string, MemberDescription[]> types, string currentTypeName)
        {
            var currentTypeMembers = types[currentTypeName];
            var currentTypeMembersEncoded = currentTypeMembers.Select(x => x.Type + " " + x.Name);
            var result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(currentTypeName, currentTypeName + "(" + string.Join(",", currentTypeMembersEncoded.ToArray()) + ")")
            };

            result.AddRange(currentTypeMembers.Select(x => x.Type.Contains("[") ? x.Type.Substring(0, x.Type.IndexOf("[")) : x.Type)
                                              .Distinct()
                                              .Where(IsReferenceType)
                                              .SelectMany(x => EncodeTypes(types, x)));
            return result;
        }
        
        internal static bool IsReferenceType(string typeName)
        {
            switch (typeName)
            {
                case var bytes when BytesPattern.IsMatch(bytes):
                case var @uint when UintPattern.IsMatch(@uint):
                case var @int when IntPattern.IsMatch(@int):
                case "bytes":
                case "string":
                case "bool":
                case "address":
                case var array when array.Contains("["):
                    return false;
                default:
                    return true;
            }
        }

        private static string FormatMemberValue(object? value)
        {
            if (value == null)
                return "<null>";

            return value switch
            {
                byte[] bytes => Convert.ToHexString(bytes),
                IEnumerable values when value is not string => "[" + string.Join(", ", values.Cast<object?>().Select(FormatMemberValue)) + "]",
                _ => value.ToString() ?? "<null>"
            };
        }
    }
}
