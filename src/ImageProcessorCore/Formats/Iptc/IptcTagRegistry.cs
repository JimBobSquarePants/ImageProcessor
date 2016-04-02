using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// A registry of Iptc Tags. The registry maintains a dictionary of Tag Id to IptcTag objects.
    /// </summary>
    public class IptcTagRegistry : IReadOnlyDictionary<int, IptcTag>
    {
        public const int TagEnvelopeRecordVersion = 0x0100;
        public const int TagDestination = 0x0105;
        public const int TagFileFormat = 0x0114;
        public const int TagFileVersion = 0x0116;
        public const int TagServiceId = 0x011E;
        public const int TagEnvelopeNumber = 0x0128;
        public const int TagProductId = 0x0132;
        public const int TagEnvelopePriority = 0x013C;
        public const int TagDateSent = 0x0146;
        public const int TagTimeSent = 0x0150;
        public const int TagCodedCharacterSet = 0x015A;
        public const int TagUniqueObjectName = 0x0164;
        public const int TagArmIdentifier = 0x0178;
        public const int TagArmVersion = 0x017a;
        public const int TagApplicationRecordVersion = 0x0200;
        public const int TagObjectTypeReference = 0x0203;
        public const int TagObjectAttributeReference = 0x0204;
        public const int TagObjectName = 0x0205;
        public const int TagEditStatus = 0x0207;
        public const int TagEditorialUpdate = 0x0208;
        public const int TagUrgency = 0X020A;
        public const int TagSubjectReference = 0X020C;
        public const int TagCategory = 0x020F;
        public const int TagSupplementalCategories = 0x0214;
        public const int TagFixtureId = 0x0216;
        public const int TagKeywords = 0x0219;
        public const int TagContentLocationCode = 0x021A;
        public const int TagContentLocationName = 0x021B;
        public const int TagReleaseDate = 0X021E;
        public const int TagReleaseTime = 0x0223;
        public const int TagExpirationDate = 0x0225;
        public const int TagExpirationTime = 0x0226;
        public const int TagSpecialInstructions = 0x0228;
        public const int TagActionAdvised = 0x022A;
        public const int TagReferenceService = 0x022D;
        public const int TagReferenceDate = 0x022F;
        public const int TagReferenceNumber = 0x0232;
        public const int TagDateCreated = 0x0237;
        public const int TagTimeCreated = 0X023C;
        public const int TagDigitalDateCreated = 0x023E;
        public const int TagDigitalTimeCreated = 0x023F;
        public const int TagOriginatingProgram = 0x0241;
        public const int TagProgramVersion = 0x0246;
        public const int TagObjectCycle = 0x024B;
        public const int TagByLine = 0x0250;
        public const int TagByLineTitle = 0x0255;
        public const int TagCity = 0x025A;
        public const int TagSubLocation = 0x025C;
        public const int TagProvinceOrState = 0x025F;
        public const int TagCountryOrPrimaryLocationCode = 0x0264;
        public const int TagCountryOrPrimaryLocationName = 0x0265;
        public const int TagOriginalTransmissionReference = 0x0267;
        public const int TagHeadline = 0x0269;
        public const int TagCredit = 0x026E;
        public const int TagSource = 0x0273;
        public const int TagCopyrightNotice = 0x0274;
        public const int TagContact = 0x0276;
        public const int TagCaption = 0x0278;
        public const int TagLocalCaption = 0x0279;
        public const int TagCaptionWriter = 0x027A;
        public const int TagRasterizedCaption = 0x027D;
        public const int TagImageType = 0x0282;
        public const int TagImageOrientation = 0x0283;
        public const int TagLanguageIdentifier = 0x0287;
        public const int TagAudioType = 0x0296;
        public const int TagAudioSamplingRate = 0x0297;
        public const int TagAudioSamplingResolution = 0x0298;
        public const int TagAudioDuration = 0x0299;
        public const int TagAudioOutcue = 0x029A;
        public const int TagJobId = 0x02B8;
        public const int TagMasterDocumentId = 0x02B9;
        public const int TagShortDocumentId = 0x02BA;
        public const int TagUniqueDocumentId = 0x02BB;
        public const int TagOwnerId = 0x02BC;
        public const int TagObjectPreviewFileFormat = 0x02C8;
        public const int TagObjectPreviewFileFormatVersion = 0x02C9;
        public const int TagObjectPreviewData = 0x02CA;

        private static readonly Lazy<IptcTagRegistry> Lazy = new Lazy<IptcTagRegistry>(() => new IptcTagRegistry());
        private readonly Dictionary<int, IptcTag> _tags;

        private IptcTagRegistry()
        {
            _tags = new Dictionary<int, IptcTag>
            {
                //      KEY                                                        // VALUE (id, name)
                { TagEnvelopeRecordVersion,             new IptcTag(TagEnvelopeRecordVersion,           "Enveloped Record Version"                  )},
                { TagDestination,                       new IptcTag(TagDestination,                     "Destination"                               )},
                { TagFileFormat,                        new IptcTag(TagFileFormat,                      "File Format"                               )},
                { TagFileVersion,                       new IptcTag(TagFileVersion,                     "File Version"                              )},
                { TagServiceId,                         new IptcTag(TagServiceId,                       "Service Identifier"                        )},
                { TagEnvelopeNumber,                    new IptcTag(TagEnvelopeNumber,                  "Envelope Number"                           )},
                { TagProductId,                         new IptcTag(TagProductId,                       "Product Identifier"                        )},
                { TagEnvelopePriority,                  new IptcTag(TagEnvelopePriority,                "Envelope Priority"                         )},
                { TagDateSent,                          new IptcTag(TagDateSent,                        "Date Sent"                                 )},
                { TagTimeSent,                          new IptcTag(TagTimeSent,                        "Time Sent"                                 )},
                { TagCodedCharacterSet,                 new IptcTag(TagCodedCharacterSet,               "Coded Character Set"                       )},
                { TagUniqueObjectName,                  new IptcTag(TagUniqueObjectName,                "Unique Object Name"                        )},
                { TagArmIdentifier,                     new IptcTag(TagArmIdentifier,                   "ARM Identifier"                            )},
                { TagArmVersion,                        new IptcTag(TagArmVersion,                      "ARM Version"                               )},
                { TagApplicationRecordVersion,          new IptcTag(TagApplicationRecordVersion,        "Application Record Version"                )},
                { TagObjectTypeReference,               new IptcTag(TagObjectTypeReference,             "Object Type Reference"                     )},
                { TagObjectAttributeReference,          new IptcTag(TagObjectAttributeReference,        "Object Attribute Reference"                )},
                { TagObjectName,                        new IptcTag(TagObjectName,                      "Object Name"                               )},
                { TagEditStatus,                        new IptcTag(TagEditStatus,                      "Edit Status"                               )},
                { TagEditorialUpdate,                   new IptcTag(TagEditorialUpdate,                 "Editorial Update"                          )},
                { TagUrgency,                           new IptcTag(TagUrgency,                         "Urgency"                                   )},
                { TagSubjectReference,                  new IptcTag(TagSubjectReference,                "Subject Reference"                         )},
                { TagCategory,                          new IptcTag(TagCategory,                        "Category"                                  )},
                { TagSupplementalCategories,            new IptcTag(TagSupplementalCategories,          "Supplemental Category(s)"                  )},
                { TagFixtureId,                         new IptcTag(TagFixtureId,                       "Fixture Identifier"                        )},
                { TagKeywords,                          new IptcTag(TagKeywords,                        "Keywords"                                  )},
                { TagContentLocationCode,               new IptcTag(TagContentLocationCode,             "Content Location Code"                     )},
                { TagContentLocationName,               new IptcTag(TagContentLocationName,             "Content Location Name"                     )},
                { TagReleaseDate,                       new IptcTag(TagReleaseDate,                     "Release Date"                              )},
                { TagReleaseTime,                       new IptcTag(TagReleaseTime,                     "Release Time"                              )},
                { TagExpirationDate,                    new IptcTag(TagExpirationDate,                  "Expiration Date"                           )},
                { TagExpirationTime,                    new IptcTag(TagExpirationTime,                  "Expiration Time"                           )},
                { TagSpecialInstructions,               new IptcTag(TagSpecialInstructions,             "Special Instructions"                      )},
                { TagActionAdvised,                     new IptcTag(TagActionAdvised,                   "Action Advised"                            )},
                { TagReferenceService,                  new IptcTag(TagReferenceService,                "Reference Service"                         )},
                { TagReferenceDate,                     new IptcTag(TagReferenceDate,                   "Reference Date"                            )},
                { TagReferenceNumber,                   new IptcTag(TagReferenceNumber,                 "Reference Number"                          )},
                { TagDateCreated,                       new IptcTag(TagDateCreated,                     "Date Created"                              )},
                { TagTimeCreated,                       new IptcTag(TagTimeCreated,                     "Time Created"                              )},
                { TagDigitalDateCreated,                new IptcTag(TagDigitalDateCreated,              "Digital Date Created"                      )},
                { TagDigitalTimeCreated,                new IptcTag(TagDigitalTimeCreated,              "Digital Time Created"                      )},
                { TagOriginatingProgram,                new IptcTag(TagOriginatingProgram,              "Originating Program"                       )},
                { TagProgramVersion,                    new IptcTag(TagProgramVersion,                  "Program Version"                           )},
                { TagObjectCycle,                       new IptcTag(TagObjectCycle,                     "Object Cycle"                              )},
                { TagByLine,                            new IptcTag(TagByLine,                          "By-line"                                   )},
                { TagByLineTitle,                       new IptcTag(TagByLineTitle,                     "By-line Title"                             )},
                { TagCity,                              new IptcTag(TagCity,                            "City"                                      )},
                { TagSubLocation,                       new IptcTag(TagSubLocation,                     "Sub-location"                              )},
                { TagProvinceOrState,                   new IptcTag(TagProvinceOrState,                 "Province/State"                            )},
                { TagCountryOrPrimaryLocationCode,      new IptcTag(TagCountryOrPrimaryLocationCode,    "Country/Primary Location Code"             )},
                { TagCountryOrPrimaryLocationName,      new IptcTag(TagCountryOrPrimaryLocationName,    "Country/Primary Location Name"             )},
                { TagOriginalTransmissionReference,     new IptcTag(TagOriginalTransmissionReference,   "Original Transmission Reference"           )},
                { TagHeadline,                          new IptcTag(TagHeadline,                        "Headline"                                  )},
                { TagCredit,                            new IptcTag(TagCredit,                          "Credit"                                    )},
                { TagSource,                            new IptcTag(TagSource,                          "Source"                                    )},
                { TagCopyrightNotice,                   new IptcTag(TagCopyrightNotice,                 "Copyright Notice"                          )},
                { TagContact,                           new IptcTag(TagContact,                         "Contact"                                   )},
                { TagCaption,                           new IptcTag(TagCaption,                         "Caption/Abstract"                          )},
                { TagLocalCaption,                      new IptcTag(TagLocalCaption,                    "Local Caption"                             )},
                { TagCaptionWriter,                     new IptcTag(TagCaptionWriter,                   "Caption Writer/Editor"                     )},
                { TagRasterizedCaption,                 new IptcTag(TagRasterizedCaption,               "Rasterized Caption"                        )},
                { TagImageType,                         new IptcTag(TagImageType,                       "Image Type"                                )},
                { TagImageOrientation,                  new IptcTag(TagImageOrientation,                "Image Orientation"                         )},
                { TagLanguageIdentifier,                new IptcTag(TagLanguageIdentifier,              "Language Identifier"                       )},
                { TagAudioType,                         new IptcTag(TagAudioType,                       "Audio Type"                                )},
                { TagAudioSamplingRate,                 new IptcTag(TagAudioSamplingRate,               "Audio Sampling Rate"                       )},
                { TagAudioSamplingResolution,           new IptcTag(TagAudioSamplingResolution,         "Audio Sampling Resolution"                 )},
                { TagAudioDuration,                     new IptcTag(TagAudioDuration,                   "Audio Duration"                            )},
                { TagAudioOutcue,                       new IptcTag(TagAudioOutcue,                     "Audio Outcue"                              )},
                { TagJobId,                             new IptcTag(TagJobId,                           "Job Identifier"                            )},
                { TagMasterDocumentId,                  new IptcTag(TagMasterDocumentId,                "Master Document Identifier"                )},
                { TagShortDocumentId,                   new IptcTag(TagShortDocumentId,                 "Short Document Identifier"                 )},
                { TagUniqueDocumentId,                  new IptcTag(TagUniqueDocumentId,                "Unique Document Identifier"                )},
                { TagOwnerId,                           new IptcTag(TagOwnerId,                         "Owner Identifier"                          )},
                { TagObjectPreviewFileFormat,           new IptcTag(TagObjectPreviewFileFormat,         "Object Data Preview File Format"           )},
                { TagObjectPreviewFileFormatVersion,    new IptcTag(TagObjectPreviewFileFormatVersion,  "Object Data Preview File Format Version"   )},
                { TagObjectPreviewData,                 new IptcTag(TagObjectPreviewData,               "Object Data Preview Data"                  )}
            };
        }

        public static IptcTagRegistry Instance => Lazy.Value;


        public IEnumerator<KeyValuePair<int, IptcTag>> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        public int Count => _tags.Count;

        public bool ContainsKey(int key)
        {
            return  _tags.ContainsKey(key);
        }

        public bool TryGetValue(int key, out IptcTag value)
        {
            return _tags.TryGetValue(key, out value);
        }

        public IptcTag this[int key] => _tags[key];

        public IEnumerable<int> Keys => _tags.Keys;
        public IEnumerable<IptcTag> Values => _tags.Values;
      
    }
}
