using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace thinsdk.neo.test
{
    class Program
    {
        static void Main(string[] args)
        {
            string wif = "L2CmHCqgeNHL1i9XFhTLzUXsdr5LGjag4d56YY98FqEi4j5d83Mv";
            var prikey = ThinNeo.Helper_NEO.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper_NEO.GetPublicKey_FromPrivateKey(prikey);
            var address = ThinNeo.Helper_NEO.GetAddress_FromPublicKey(pubkey);
            var datastr = "010203ff1122abcd";
            var data = ThinNeo.Helper.HexString2Bytes(datastr);
            var sign = ThinNeo.Helper_NEO.Sign(data, prikey);
            var signstr = ThinNeo.Helper.Bytes2HexString(sign);
            var check = ThinNeo.Helper_NEO.VerifySignature(data, sign, pubkey);


            Console.WriteLine("wif=" + wif);
            Console.WriteLine("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
            Console.WriteLine("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
            Console.WriteLine("address=" + address);
            Console.WriteLine("data=" + datastr);
            Console.WriteLine("signstr=" + signstr);
            Console.WriteLine("check=" + check);

            Console.ReadLine();
        }
    }
}
