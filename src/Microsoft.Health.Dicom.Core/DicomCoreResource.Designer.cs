﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DicomCoreResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DicomCoreResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Health.Dicom.Core.DicomCoreResource", typeof(DicomCoreResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Dicom Tag Property {0} must be specified and must not be null, empty or whitespace..
        /// </summary>
        internal static string AddExtendedQueryTagEntryPropertyNotSpecified {
            get {
                return ResourceManager.GetString("AddExtendedQueryTagEntryPropertyNotSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The limit must be between 1 and {0}..
        /// </summary>
        internal static string ChangeFeedLimitOutOfRange {
            get {
                return ResourceManager.GetString("ChangeFeedLimitOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The offset cannot be a negative value..
        /// </summary>
        internal static string ChangeFeedOffsetCannotBeNegative {
            get {
                return ResourceManager.GetString("ChangeFeedOffsetCannotBeNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The prefix used to identify custom audit headers cannot be empty..
        /// </summary>
        internal static string CustomHeaderPrefixCannotBeEmpty {
            get {
                return ResourceManager.GetString("CustomHeaderPrefixCannotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data store operation failed..
        /// </summary>
        internal static string DataStoreOperationFailed {
            get {
                return ResourceManager.GetString("DataStoreOperationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dicom element &apos;{0}&apos; with value &apos;{1}&apos; does not validate VR &apos;{2}&apos;: {3}..
        /// </summary>
        internal static string DicomElementValidationFailed {
            get {
                return ResourceManager.GetString("DicomElementValidationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The dicom file in the request section exceeded the allowed limit of {0} bytes..
        /// </summary>
        internal static string DicomFileLengthLimitExceeded {
            get {
                return ResourceManager.GetString("DicomFileLengthLimitExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The values for StudyInstanceUID, SeriesInstanceUID, SOPInstanceUID must be unique..
        /// </summary>
        internal static string DuplicatedUidsNotAllowed {
            get {
                return ResourceManager.GetString("DuplicatedUidsNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The extended query tag &apos;{0}&apos; has been specified before..
        /// </summary>
        internal static string DuplicateExtendedQueryTag {
            get {
                return ResourceManager.GetString("DuplicateExtendedQueryTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Duplicate AttributeId &apos;{0}&apos;, Each attribute is allowed to be specified once..
        /// </summary>
        internal static string DuplicateQueryParam {
            get {
                return ResourceManager.GetString("DuplicateQueryParam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are {0} roles with the name &apos;{1}&apos;.
        /// </summary>
        internal static string DuplicateRoleNames {
            get {
                return ResourceManager.GetString("DuplicateRoleNames", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error validating roles:
        ///{0}.
        /// </summary>
        internal static string ErrorValidatingRoles {
            get {
                return ResourceManager.GetString("ErrorValidatingRoles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Extended Query Tag feature is disabled..
        /// </summary>
        internal static string ExtendedQueryTagFeatureDisabled {
            get {
                return ResourceManager.GetString("ExtendedQueryTagFeatureDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified extended query tag with tag path {0} cannot be found..
        /// </summary>
        internal static string ExtendedQueryTagNotFound {
            get {
                return ResourceManager.GetString("ExtendedQueryTagNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One or more extended query tags already exist..
        /// </summary>
        internal static string ExtendedQueryTagsAlreadyExists {
            get {
                return ResourceManager.GetString("ExtendedQueryTagsAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Extended query tags exceeds max allowed count &apos;{0}&apos;..
        /// </summary>
        internal static string ExtendedQueryTagsExceedsMaxAllowedCount {
            get {
                return ResourceManager.GetString("ExtendedQueryTagsExceedsMaxAllowedCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Authorization failed..
        /// </summary>
        internal static string Forbidden {
            get {
                return ResourceManager.GetString("Forbidden", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified frame cannot be found..
        /// </summary>
        internal static string FrameNotFound {
            get {
                return ResourceManager.GetString("FrameNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. IncludeField has unknown attribute &apos;{0}&apos;..
        /// </summary>
        internal static string IncludeFieldUnknownAttribute {
            get {
                return ResourceManager.GetString("IncludeFieldUnknownAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The DICOM instance already exists..
        /// </summary>
        internal static string InstanceAlreadyExists {
            get {
                return ResourceManager.GetString("InstanceAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified instance cannot be found..
        /// </summary>
        internal static string InstanceNotFound {
            get {
                return ResourceManager.GetString("InstanceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified date range &apos;{0}&apos; is invalid.
        ///The first part date {1} should be lesser than or equal to the second part date {2}..
        /// </summary>
        internal static string InvalidDateRangeValue {
            get {
                return ResourceManager.GetString("InvalidDateRangeValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified Date value &apos;{0}&apos; is invalid for parameter &apos;{1}&apos;. Date should be valid and formatted as yyyyMMdd..
        /// </summary>
        internal static string InvalidDateValue {
            get {
                return ResourceManager.GetString("InvalidDateValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DICOM Identifier &apos;{0}&apos; value &apos;{1}&apos; is invalid. Value length should not exceed the maximum length of 64 characters. Value should contain characters in &apos;0&apos;-&apos;9&apos; and &apos;.&apos;. Each component must start with non-zero number..
        /// </summary>
        internal static string InvalidDicomIdentifier {
            get {
                return ResourceManager.GetString("InvalidDicomIdentifier", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The DICOM instance is invalid..
        /// </summary>
        internal static string InvalidDicomInstance {
            get {
                return ResourceManager.GetString("InvalidDicomInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input Dicom Tag Level &apos;{0}&apos; is invalid. It must have value &apos;Study&apos;, &apos;Series&apos; or &apos;Instance&apos;..
        /// </summary>
        internal static string InvalidDicomTagLevel {
            get {
                return ResourceManager.GetString("InvalidDicomTagLevel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified value &apos;{0}&apos; extended query tag with path &apos;{1}&apos; is not a valid double value..
        /// </summary>
        internal static string InvalidDoubleValue {
            get {
                return ResourceManager.GetString("InvalidDoubleValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The extended query tag &apos;{0}&apos; is invalid as it cannot be parsed into a valid Dicom Tag..
        /// </summary>
        internal static string InvalidExtendedQueryTag {
            get {
                return ResourceManager.GetString("InvalidExtendedQueryTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified frames value is not valid. At least one frame must be present, and all requested frames must have value greater than 0..
        /// </summary>
        internal static string InvalidFramesValue {
            get {
                return ResourceManager.GetString("InvalidFramesValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified fuzzymatch value &apos;{0}&apos; is not a valid boolean.
        /// </summary>
        internal static string InvalidFuzzyMatchValue {
            get {
                return ResourceManager.GetString("InvalidFuzzyMatchValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified limit value &apos;{0}&apos; is not a valid integer..
        /// </summary>
        internal static string InvalidLimitValue {
            get {
                return ResourceManager.GetString("InvalidLimitValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified value &apos;{0}&apos; extended query tag with path &apos;{1}&apos; is not a valid long value..
        /// </summary>
        internal static string InvalidLongValue {
            get {
                return ResourceManager.GetString("InvalidLongValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified offset value &apos;{0}&apos; is not a valid integer..
        /// </summary>
        internal static string InvalidOffsetValue {
            get {
                return ResourceManager.GetString("InvalidOffsetValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The query string included invalid characters..
        /// </summary>
        internal static string InvalidQueryString {
            get {
                return ResourceManager.GetString("InvalidQueryString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The following query parameter is invalid: {0}..
        /// </summary>
        internal static string InvalidQueryStringValue {
            get {
                return ResourceManager.GetString("InvalidQueryStringValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified Transfer Syntax value is not valid..
        /// </summary>
        internal static string InvalidTransferSyntaxValue {
            get {
                return ResourceManager.GetString("InvalidTransferSyntaxValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The VR code &apos;{0}&apos; for tag &apos;{1}&apos; is invalid..
        /// </summary>
        internal static string InvalidVRCode {
            get {
                return ResourceManager.GetString("InvalidVRCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified item cannot be found..
        /// </summary>
        internal static string ItemNotFound {
            get {
                return ResourceManager.GetString("ItemNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The StudyInstanceUid &apos;{0}&apos; in the payload does not match the specified StudyInstanceUid &apos;{1}&apos;..
        /// </summary>
        internal static string MismatchStudyInstanceUid {
            get {
                return ResourceManager.GetString("MismatchStudyInstanceUid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The extended query tag(s) is missing..
        /// </summary>
        internal static string MissingExtendedQueryTag {
            get {
                return ResourceManager.GetString("MissingExtendedQueryTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The level for extended query tag &apos;{0}&apos; is missing..
        /// </summary>
        internal static string MissingLevel {
            get {
                return ResourceManager.GetString("MissingLevel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The private creator for private tag &apos;{0}&apos; is missing..
        /// </summary>
        internal static string MissingPrivateCreator {
            get {
                return ResourceManager.GetString("MissingPrivateCreator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The request body is missing..
        /// </summary>
        internal static string MissingRequestBody {
            get {
                return ResourceManager.GetString("MissingRequestBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The required tag &apos;{0}&apos; is missing..
        /// </summary>
        internal static string MissingRequiredTag {
            get {
                return ResourceManager.GetString("MissingRequiredTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The vr for tag &apos;{0}&apos; is missing..
        /// </summary>
        internal static string MissingVRCode {
            get {
                return ResourceManager.GetString("MissingVRCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The request headers are not acceptable.
        /// </summary>
        internal static string NotAcceptableHeaders {
            get {
                return ResourceManager.GetString("NotAcceptableHeaders", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The request contains multiple accept headers, which is not supported..
        /// </summary>
        internal static string NotSupportMultipleAcceptHeaders {
            get {
                return ResourceManager.GetString("NotSupportMultipleAcceptHeaders", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The private creator is not empty for standard tag &apos;{0}&apos;..
        /// </summary>
        internal static string PrivateCreatorNotEmpty {
            get {
                return ResourceManager.GetString("PrivateCreatorNotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The private creator is not empty for private identification code &apos;{0}&apos;..
        /// </summary>
        internal static string PrivateCreatorNotEmptyForPrivateIdentificationCode {
            get {
                return ResourceManager.GetString("PrivateCreatorNotEmptyForPrivateIdentificationCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The private creator for tag &apos;{0}&apos; is not a valid LO attribute..
        /// </summary>
        internal static string PrivateCreatorNotValidLO {
            get {
                return ResourceManager.GetString("PrivateCreatorNotValidLO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. AttributeId &apos;{0}&apos; has empty string value that is not supported..
        /// </summary>
        internal static string QueryEmptyAttributeValue {
            get {
                return ResourceManager.GetString("QueryEmptyAttributeValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Querying is only supported at resource level Studies/Series/Instances..
        /// </summary>
        internal static string QueryInvalidResourceLevel {
            get {
                return ResourceManager.GetString("QueryInvalidResourceLevel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query.  Specified limit value {0} is outside the allowed range of {1}..{2}..
        /// </summary>
        internal static string QueryResultCountMaxExceeded {
            get {
                return ResourceManager.GetString("QueryResultCountMaxExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The query tag &apos;{0}&apos; is already supported..
        /// </summary>
        internal static string QueryTagAlreadySupported {
            get {
                return ResourceManager.GetString("QueryTagAlreadySupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequential dicom tags are currently not supported..
        /// </summary>
        internal static string SequentialDicomTagsNotSupported {
            get {
                return ResourceManager.GetString("SequentialDicomTagsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified series cannot be found..
        /// </summary>
        internal static string SeriesInstanceNotFound {
            get {
                return ResourceManager.GetString("SeriesInstanceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified series cannot be found..
        /// </summary>
        internal static string SeriesNotFound {
            get {
                return ResourceManager.GetString("SeriesNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The server is currently unable to receive requests. Please retry your request. If the issue persists, please contact support..
        /// </summary>
        internal static string ServiceUnavailable {
            get {
                return ResourceManager.GetString("ServiceUnavailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified study cannot be found..
        /// </summary>
        internal static string StudyInstanceNotFound {
            get {
                return ResourceManager.GetString("StudyInstanceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified study cannot be found..
        /// </summary>
        internal static string StudyNotFound {
            get {
                return ResourceManager.GetString("StudyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Unknown query parameter &apos;{0}&apos;. If the parameter is an attribute keyword, check the casing as they are case-sensitive. The conformance statement has a list of supported query parameters and queryable attributes..
        /// </summary>
        internal static string UnknownQueryParameter {
            get {
                return ResourceManager.GetString("UnknownQueryParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified content type &apos;{0}&apos; is not supported..
        /// </summary>
        internal static string UnsupportedContentType {
            get {
                return ResourceManager.GetString("UnsupportedContentType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. AttributeId {0} is not queryable. .
        /// </summary>
        internal static string UnsupportedSearchParameter {
            get {
                return ResourceManager.GetString("UnsupportedSearchParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified transcoding is not supported..
        /// </summary>
        internal static string UnsupportedTranscoding {
            get {
                return ResourceManager.GetString("UnsupportedTranscoding", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The VR code &apos;{0}&apos; specified for tag &apos;{1}&apos; is not supported..
        /// </summary>
        internal static string UnsupportedVRCode {
            get {
                return ResourceManager.GetString("UnsupportedVRCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The VR code &apos;{0}&apos; is incorrectly specified for &apos;{1}&apos;. The expected VR code for it is &apos;{2}&apos;. Retry this request either with the correct VR code or without specifying it..
        /// </summary>
        internal static string UnsupportedVRCodeOnTag {
            get {
                return ResourceManager.GetString("UnsupportedVRCodeOnTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value contains invalid character..
        /// </summary>
        internal static string ValueContainsInvalidCharacter {
            get {
                return ResourceManager.GetString("ValueContainsInvalidCharacter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value contains more than 5 components..
        /// </summary>
        internal static string ValueExceedsAllowedComponents {
            get {
                return ResourceManager.GetString("ValueExceedsAllowedComponents", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value contains more than 3 groups..
        /// </summary>
        internal static string ValueExceedsAllowedGroups {
            get {
                return ResourceManager.GetString("ValueExceedsAllowedGroups", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value cannot be parsed as a valid date..
        /// </summary>
        internal static string ValueIsInvalidDate {
            get {
                return ResourceManager.GetString("ValueIsInvalidDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value length is above {0}..
        /// </summary>
        internal static string ValueLengthAboveMaxLength {
            get {
                return ResourceManager.GetString("ValueLengthAboveMaxLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value length is below {0}..
        /// </summary>
        internal static string ValueLengthBelowMinLength {
            get {
                return ResourceManager.GetString("ValueLengthBelowMinLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value exceeds maximum length of 16 characters..
        /// </summary>
        internal static string ValueLengthExceeds16Characters {
            get {
                return ResourceManager.GetString("ValueLengthExceeds16Characters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value exceeds maximum length of 64 characters..
        /// </summary>
        internal static string ValueLengthExceeds64Characters {
            get {
                return ResourceManager.GetString("ValueLengthExceeds64Characters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value length is not {0}..
        /// </summary>
        internal static string ValueLengthIsNotRequiredLength {
            get {
                return ResourceManager.GetString("ValueLengthIsNotRequiredLength", resourceCulture);
            }
        }
    }
}
