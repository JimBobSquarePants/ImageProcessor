namespace ImageProcessor.Imaging
{
    using System;

    public class ComputerArchitectureInfo : IComputerArchitectureInfo
    {
        public bool IsLittleEndian()
        {
            return BitConverter.IsLittleEndian;
        }
    }
}