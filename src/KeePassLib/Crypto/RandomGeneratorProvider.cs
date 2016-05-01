using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePass.Crypto
{
    public class RandomGeneratorProvider : IRandomGeneratorProvider
    {
        private readonly IHashProvider _hashProvider;

        public RandomGeneratorProvider(IHashProvider hashProvider)
        {
            _hashProvider = hashProvider;
        }

        public IRandomGenerator Get(CrsAlgorithm algorithm, byte[] protectedStreamKey)
        {
            switch (algorithm)
            {
                case CrsAlgorithm.ArcFourVariant:
                    return new Rc4RandomGenerator(protectedStreamKey);
                default:
                    return new Salsa20RandomGenerator(_hashProvider, protectedStreamKey);
            }
        }
    }
}
