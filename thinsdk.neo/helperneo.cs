using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThinNeo
{
    public static class Helper_NEO
    {
        public static Hash256 CalcHash256(byte[] data)
        {
            var hash1 = Helper.Sha256.ComputeHash(data);
            var hash2 = Helper.Sha256.ComputeHash(hash1);
            return hash2;
        }
        public static Hash160 CalcHash160(byte[] data)
        {
            var hash1 = Helper.Sha256.ComputeHash(data);
            var hash2 = Helper.RIPEMD160.ComputeHash(hash1);
            return hash2;
        }
        public static string GetWifFromPrivateKey(byte[] prikey)
        {
            if (prikey.Length != 32)
                throw new Exception("error prikey.");
            byte[] data = new byte[34];
            data[0] = 0x80;
            data[33] = 0x01;
            for (var i = 0; i < 32; i++)
            {
                data[i + 1] = prikey[i];
            }
            byte[] checksum = Helper.Sha256.ComputeHash(data);
            checksum = Helper.Sha256.ComputeHash(checksum);
            checksum = checksum.Take(4).ToArray();
            byte[] alldata = data.Concat(checksum).ToArray();
            string wif = Cryptography.Base58.Encode(alldata);
            return wif;
        }
        public static byte[] GetPrivateKeyFromWIF(string wif)
        {
            if (wif == null) throw new ArgumentNullException();
            byte[] data = Cryptography.Base58.Decode(wif);
            //检查标志位
            if (data.Length != 38 || data[0] != 0x80 || data[33] != 0x01)
                throw new Exception("wif length or tag is error");
            //取出检验字节
            var sum = data.Skip(data.Length - 4);
            byte[] realdata = data.Take(data.Length - 4).ToArray();

            //验证,对前34字节进行进行两次hash取前4个字节
            byte[] checksum = Helper.Sha256.ComputeHash(realdata);
            checksum = Helper.Sha256.ComputeHash(checksum);
            var sumcalc = checksum.Take(4);
            if (sum.SequenceEqual(sumcalc) == false)
                throw new Exception("the sum is not match.");

            byte[] privateKey = new byte[32];
            Buffer.BlockCopy(data, 1, privateKey, 0, privateKey.Length);
            Array.Clear(data, 0, data.Length);
            return privateKey;
        }
        public static byte[] GetPublicKey_FromPrivateKey(byte[] privateKey)
        {
            var PublicKey = ThinNeo.Cryptography.ECC.ECCurve.Secp256r1.G * privateKey;
            return PublicKey.EncodePoint(true);
        }
        public static byte[] GetAddressScript_FromPublicKey(byte[] publicKey)
        {
            byte[] script = new byte[publicKey.Length + 2];
            script[0] = (byte)publicKey.Length;
            Array.Copy(publicKey, 0, script, 1, publicKey.Length);
            script[script.Length - 1] = 172;//CHECKSIG
            return script;
        }
        public static Hash160 GetScriptHash_FromPublicKey(byte[] publicKey)
        {
            byte[] script = GetAddressScript_FromPublicKey(publicKey);
            return CalcHash160(script);
        }
        public static string GetAddress_FromScriptHash(Hash160 scripthash)
        {
            byte[] data = new byte[20 + 1];
            data[0] = 0x17;
            Array.Copy(scripthash, 0, data, 1, 20);
            var hash = Helper.Sha256.ComputeHash(data);
            hash = Helper.Sha256.ComputeHash(hash);

            var alldata = data.Concat(hash.Take(4)).ToArray();

            return ThinNeo.Cryptography.Base58.Encode(alldata);
        }
        public static string GetAddress_FromPublicKey(byte[] publicKey)
        {
            var script = GetAddressScript_FromPublicKey(publicKey);
            var hash = CalcHash160(script);
            var address = GetAddress_FromScriptHash(hash);
            return address;
        }
        public static Hash160 GetScriptHash_FromAddress(string address)
        {
            var alldata = ThinNeo.Cryptography.Base58.Decode(address);
            if (alldata.Length != 25)
                throw new Exception("error length.");
            var data = alldata.Take(alldata.Length - 4).ToArray();
            if (data[0] != 0x17)
                throw new Exception("not a address");
            var hash = Helper.Sha256.ComputeHash(data);
            hash = Helper.Sha256.ComputeHash(hash);
            var hashbts = hash.Take(4).ToArray();
            var datahashbts = alldata.Skip(alldata.Length - 4).ToArray();
            if (hashbts.SequenceEqual(datahashbts) == false)
                throw new Exception("not match hash");
            var pkhash = data.Skip(1).ToArray();
            return new Hash160(pkhash);
        }
        public static Hash160 GetScriptHash_FromAddress_WithoutCheck(string address)
        {
            var alldata = ThinNeo.Cryptography.Base58.Decode(address);
            if (alldata.Length != 25)
                throw new Exception("error length.");
            if (alldata[0] != 0x17)
                throw new Exception("not a address");
            var data = alldata.Take(alldata.Length - 4).ToArray();
            var pkhash = data.Skip(1).ToArray();
            return new Hash160(pkhash);
        }
        public static string GetNep2FromPrivateKey(byte[] prikey, string passphrase)
        {
            var pubkey = GetPublicKey_FromPrivateKey(prikey);
            var script_hash = GetScriptHash_FromPublicKey(pubkey);

            string address = GetAddress_FromScriptHash(script_hash);

            var b1 = Helper.CalcSha256(Encoding.ASCII.GetBytes(address));
            var b2 = Helper.CalcSha256(b1);
            byte[] addresshash = b2.Take(4).ToArray();
            byte[] derivedkey = Cryptography.SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, 16384, 8, 8, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            var xorinfo = Helper.XOR(prikey, derivedhalf1);
            byte[] encryptedkey = Helper.AES256Encrypt(xorinfo, derivedhalf2);
            byte[] buffer = new byte[39];
            buffer[0] = 0x01;
            buffer[1] = 0x42;
            buffer[2] = 0xe0;
            Buffer.BlockCopy(addresshash, 0, buffer, 3, addresshash.Length);
            Buffer.BlockCopy(encryptedkey, 0, buffer, 7, encryptedkey.Length);
            return Helper.Base58CheckEncode(buffer);
        }
        public static byte[] GetPrivateKeyFromNEP2(string nep2, string passphrase, int N = 16384, int r = 8, int p = 8)
        {
            if (nep2 == null) throw new ArgumentNullException(nameof(nep2));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            byte[] data = Helper.Base58CheckDecode(nep2);
            if (data.Length != 39 || data[0] != 0x01 || data[1] != 0x42 || data[2] != 0xe0)
                throw new FormatException();
            byte[] addresshash = new byte[4];
            Buffer.BlockCopy(data, 3, addresshash, 0, 4);
            byte[] derivedkey = Cryptography.SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, N, r, p, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            byte[] encryptedkey = new byte[32];
            Buffer.BlockCopy(data, 7, encryptedkey, 0, 32);
            byte[] prikey = Helper.XOR(Helper.AES256Decrypt(encryptedkey, derivedhalf2), derivedhalf1);
            var pubkey = GetPublicKey_FromPrivateKey(prikey);
            var address = GetAddress_FromPublicKey(pubkey);
            var hash = Helper.CalcSha256(Encoding.ASCII.GetBytes(address));
            hash = Helper.CalcSha256(hash);
            for (var i = 0; i < 4; i++)
            {
                if (hash[i] != addresshash[i])
                    throw new Exception("check error.");
            }
            //Cryptography.ECC.ECPoint pubkey = Cryptography.ECC.ECCurve.Secp256r1.G * prikey;
            //UInt160 script_hash = Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash();
            //string address = ToAddress(script_hash);
            //if (!Encoding.ASCII.GetBytes(address).Sha256().Sha256().Take(4).SequenceEqual(addresshash))
            //    throw new FormatException();
            return prikey;


        }
    }
}
