namespace ImageProcessorCore.Formats.Tiff
{
    public enum BitOrderMask : byte
    {
        LittleEndianHigh = 0x49,
        LittleEndianLow = 0x49,
        BigEndianHigh = 0x4D,
        BigEndianLow = 0x4D
    }

    public enum BitOrder {  BigEndian, LittleEndian }

}
