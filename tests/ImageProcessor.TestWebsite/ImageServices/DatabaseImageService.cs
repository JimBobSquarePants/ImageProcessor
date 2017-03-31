// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseImageService.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   A demo non-file-local service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.TestWebsite.ImageServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ImageProcessor.Web.Services;

    /// <summary>
    /// A demo non-file-local service.
    /// </summary>
    public class DatabaseImageService : IImageService
    {
        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = "database.axd";

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        public bool IsFileLocalService => false;

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the white list of <see cref="System.Uri"/>. 
        /// </summary>
        public Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public bool IsValidRequest(string path)
        {
            // Just return true for now. Devlopers should add their own logic.
            return true;
        }

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">
        /// The value identifying the image to fetch.
        /// </param>
        /// <returns>
        /// The <see cref="byte"/> array containing the image data.
        /// </returns>
        public Task<byte[]> GetImage(object id)
        {
            // No database work here, that's down to individual implementations
            // A 64 bit encoded image.
            string img = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAADAFBMVEUIGyUIHCkIGigIGicIGyjtcSUIW6fLIC01qE"
                         + "kGnX7lSyf3qxv87iG20TQ/L4R+KYJfLIMJGygILTQlIigIJDoWIUAmHz9JUyciHClIQiUfIEAvlUUmJijKIC4IHCk"
                         + "LHCsILEkPLy05HSooQpMfKSdUs0QMHii1XCbzkiCwzzUKHigUHSgJJC1GJ2k1SyuYJ2bzkSDzkx8IHS0QGygOHS8Z"
                         + "ICgNLS0YLCklJVhOLYRtJ3ZxKoOZJmWsJFDZNiofo2IlpForpFLoVSfqYSZ6vj6ZtDL77SEPISgTHTQVIyckIUUvI"
                         + "EY/Jyg7JV1FIlYIPWxMKigHTEk4LXg6LXwHVE4ITYxqMCiMHywHXlUiRpYHZFl/KYIIX6W5IC0IaJsIcJtNYy3OJC"
                         + "0HepMHfpK1QCgHj4ctjULbOykJnnriRigyqEyieCDNZSZ4kDDmbCWGoTHtbyW+iR7YmR3zkyCnwTP3txzVzSL5vR3"
                         + "C1jD6zR7p5ib37CIIIzgYIEAdHz4LKUcjHz8bIkgiJSgPMC0dI0w0HTAeLicIOj0iMic2LCgWM2UIOmZbHSpAI1Uw"
                         + "Kms1NiZmHisdNnAWRzJYIUphID5vHitGNSc8OyUtQitTJGJYLShNNScwRStBLX5DL4RJLoQIT4wcWDY6TStaLYMiR5"
                         + "ZISiVgLIN1MygIVZugHyxoK4N7NChxKXx1KHtoPydHUSYIWaMIW6ZGVCh2KoNsQSdYTCR9KYIlVI4KbVpMViYiajs7"
                         + "YTFhUSTBIC1sTiRlUyORPCcHd2okcjydPCiuL04Hf2oHgWupPCiLTCYnez/KLCsHg23RKizTLStXbS4Hg4+WUSbVMS"
                         + "uDZiIGlYOmViYGm3/fQSkPn3WRbiF2eyVpgC93eyUUoG8YoWvlTCdCm0LmTic0pEjEYibpXSbaaiXmZCbhaCbrZybs"
                         + "aSaknySjoiXNkh3dmx2Gwj7inhzymiCtyDT0qRv3rBv3rxu30jS50jPxwR7G2DDO1yz5yB7U3izX3yvY3yv72h/73y"
                         + "D86iGl5sJoAAAABHRSTlM8Ptzea32c2QAAA01JREFUeNrt1ndUlXUcx3GSz73PlTuf0hCUewnpVjKUvWRvBRLITAlUE"
                         + "NBQTFNylKOhtstyNDXTpqamZcu992q7tWxbuU0zv8/yec65wA/O/cfj4f0Pf31e53t+57nn4HGDJ9zI08OD9u4JcLM"
                         + "WoAW4poHXQkND53hfbZ4PxQNOH7HpT7CAUDN1Jsuk9KCR+hIINAqF6W5jAJ0yzUJjrgKnhN2JXAl4QMcEdpjFLin7y"
                         + "0ax7wgQ9kwg5m8JOKcAoyXgn+cIuF3HBoaYpU6P1T4BNSyQ9g0B0e3lDokPoH2E/08a5f6gvVDt4EdcgFsMSrfeoQD"
                         + "/SsB/RqUuOqUOjQCqMEoCLshzup8BuAhjTVn0MYxU92xAK4z44aMpTuekhXuGq/czAVUY0c8JOf5jImjPBLTC0BhQz"
                         + "n3fdwKVW0P7ZgGbxR0+M5m8pSM2sYDo1Ps0+yra95sJ/GYy/QrMWEnCV9r9/a7Aja0H7k9OlfdHHgXezDAPzbxI3/H"
                         + "vw4xhHwJRPyrjn4+led1UDyA08ECKcMjnAKYaMuhLqKsbKfx+XwWwVFj/kp520IuqD1D6NjklGlSVIWNIDBBYE6Y7y"
                         + "gMIOp6edlgYMwBqK4TaG9YA2dnALt1gHtRaecwG+gD2yZ8mpwbji/NnVyCqNn1VHhF3Nxm4C7iH/mxAcMqff/0UhAG0"
                         + "eAfo0WSgUALWI7h82bqkKBH4AMhrzgXBhasrysLxZFzCS4jYkrTxlaDmXNAHlP1d/Z1ATg4wjSvlgaa+wdeV5WWzIdRT"
                         + "3zvWATgKErn3INRrW9I3DKCyou+9er1+8UMA/PVzLZbXQ0LmW0sSuQAAEcs5juu/vXpQA8DenTSWo9vxVG9LUXGczZawI"
                         + "L4k8XkAT3Ny/XdXD3IF2ug1LWoD2GMn8o4Qmy3EwXcv4IGgUk5Tu8YAqmc4xLrZbN0gxvfimIC2F+wQcuTnOyD2ol+zgI"
                         + "6WZyOhyfGytbMfE9DuLZbix+yQ47vHW60kMAHtniqK9Y202yPHPSPMVYEJ0F5ulk0sgcaqwARorxQnAe9blbr6MQF1T70l"
                         + "AW9/ohFYgHb/hk1uidVFcAXC/cXGt1V72Fduws1qjweIRbT8p9oCXG+AJ9yqlQcJbu2vAKGAALs12UmPAAAAAElFTkSuQmCC";

            return Task.Run(() => Convert.FromBase64String(img));
        }
    }
}