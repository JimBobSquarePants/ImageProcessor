using System;
using System.IO;
using System.Text;
using System.Linq;

namespace ImageProcessorCore.Formats
{
    internal class IptcDecoder
    {
        private IptcReader _reader;
        private const byte IptcMarkerByte = 0x1c;
        private readonly int _iptcLength;
        private int _offset;

        public static IptcDecoder Create(Stream stream, int iptcLength)
        {
            
            IptcReader reader = new IptcReader(stream);

            try
            {
                if (IptcMarkerByte != reader.ReadByte())
                {
                    return null;
                }

                return new IptcDecoder(reader, iptcLength);
            }
            finally
            {
                // leave the stream exactly like we found it
                reader.Seek(-1, SeekOrigin.Current);
            }
            
        }

        public IptcDecoder(IptcReader reader, int iptcLength)
        {
            _offset = 0;
            _reader = reader;
            _iptcLength = iptcLength;
        }

     
        private static void ProcessTag(IptcReader reader, IptcDirectory directory, int directoryType, int tagType, int tagByteCount)
        {
            var tagIdentifier = tagType | (directoryType << 8);
            if (tagByteCount == 0)
            {
                if (!IptcTagRegistry.Instance.ContainsKey(tagIdentifier))
                {
                    directory.Errors.Add($"Failed to find IPTC Tag {tagIdentifier}");
                    return;
                }

                IptcProperty property = new IptcProperty
                                            {
                                                Value = string.Empty,
                                                Tag = IptcTagRegistry.Instance[tagIdentifier]
                                            };
                directory.Properties.Add(property);
                return;
            }

            string str = null;

            switch (tagIdentifier)
            {
                case IptcTagRegistry.TagCodedCharacterSet:
                {
                    byte[] bytes = reader.ReadBytes(tagByteCount);
                
                    var charset = Iso2022Converter.ConvertEscapeSequenceToEncodingName(bytes);
                    if (charset == null)
                    {
                        str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        break;
                    }

                    IptcProperty property = new IptcProperty
                                                {
                                                    Value = charset,
                                                    Tag = IptcTagRegistry.Instance[tagIdentifier]
                                                };
                    directory.Properties.Add(property);
                    return;
                }
                case IptcTagRegistry.TagEnvelopeRecordVersion:
                case IptcTagRegistry.TagApplicationRecordVersion:
                case IptcTagRegistry.TagFileVersion:
                case IptcTagRegistry.TagArmVersion:
                case IptcTagRegistry.TagProgramVersion:
                {
                    // short
                    if (tagByteCount >= 2)
                    {
                        var shortValue = reader.ReadUInt16();
                        reader.Seek(tagByteCount - 2, SeekOrigin.Current);

                        IptcProperty property = new IptcProperty
                                {
                                    Value = shortValue,
                                    Tag = IptcTagRegistry.Instance[tagIdentifier]
                                };
                                directory.Properties.Add(property);
                        return;
                    }
                    break;
                }
                case IptcTagRegistry.TagUrgency:
                {
                    IptcProperty property = new IptcProperty
                    {
                        Value = reader.ReadByte(),
                        Tag = IptcTagRegistry.Instance[tagIdentifier]
                    };
                    directory.Properties.Add(property);
                    reader.Seek(tagByteCount - 1, SeekOrigin.Current);
                    return;
                }
               
            }

            // If we haven't returned yet, treat it as a string
            // NOTE that there's a chance we've already loaded the value as a string above, but failed to parse the value
            if (str == null)
            {
                string encodingName = (string) directory.Properties
                    .First( i => i.Tag.Id == IptcTagRegistry.TagCodedCharacterSet).Value;
                Encoding encoding = null;
                if (encodingName != null)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(encodingName);
                    }
                    catch (ArgumentException)
                    { }
                }

                byte[] bytes = reader.ReadBytes(tagByteCount);

                if (encoding == null)
                    encoding = Iso2022Converter.GuessEncoding(bytes);

                if (encoding == null)
                    encoding = Encoding.UTF8;

                str = encoding.GetString(bytes, 0, bytes.Length);
            }

            //TODO: Figure out this nonsense....
            //if (directory.ContainsTag(tagIdentifier))
            //{
            //    // this fancy string[] business avoids using an ArrayList for performance reasons
            //    var oldStrings = directory.GetStringArray(tagIdentifier);

            //    string[] newStrings;
            //    if (oldStrings == null)
            //    {
            //        // TODO hitting this block means any prior value(s) are discarded
            //        newStrings = new string[1];
            //    }
            //    else
            //    {
            //        newStrings = new string[oldStrings.Length + 1];
            //        Array.Copy(oldStrings, 0, newStrings, 0, oldStrings.Length);
            //    }
            //    newStrings[newStrings.Length - 1] = str;
            //    directory.Set(tagIdentifier, newStrings);
            //}
            // else
            // {

            IptcProperty p = new IptcProperty
            {
                Value = str,
                Tag = IptcTagRegistry.Instance[tagIdentifier]
            };
            directory.Properties.Add(p);
        }


        public IptcDirectory Decode()
        {
            IptcDirectory directory = new IptcDirectory();

            do
            {
                // identifies start of a tag
                if (IptcMarkerByte != _reader.ReadByte())
                {
                    break;
                }

                _offset++;

                // must have another 5 bytes to have a valid tag
                if (_offset + 5 >= _iptcLength)
                {
                    break;
                }

                int directoryType;
                int tagType;
                int tagByteCount;

                try
                {
                    directoryType = _reader.ReadByte();
                    tagType = _reader.ReadByte();
                    // TODO support Extended DataSet Tag (see 1.5(c), p14, IPTC-IIMV4.2.pdf)
                    tagByteCount = _reader.ReadUInt16();
                    _offset += 4;

                  
                }
                catch (IOException)
                {
                    directory.Errors.Add("IPTC data segment ended mid-way through tag descriptor");
                    break;
                }

                if (_offset + tagByteCount > _iptcLength)
                {
                    directory.Errors.Add("Data for tag extends beyond end of IPTC segment");
                    break;
                }

                ProcessTag(_reader, directory, directoryType, tagType, tagByteCount);

                _offset += tagByteCount;

            } while (_offset < _iptcLength);

            return directory;
        }

    }
}
