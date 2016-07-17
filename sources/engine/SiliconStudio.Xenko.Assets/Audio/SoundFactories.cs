using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.Audio
{
    public class MusicSoundFactory : AssetFactory<SoundAsset>
    {
        public override SoundAsset New()
        {
            return new SoundAsset { CompressionRatio = 10, SampleRate = 44100, Spatialized = false, StreamFromDisk = true };
        }
    }

    public class SpatializedSoundFactory : AssetFactory<SoundAsset>
    {
        public override SoundAsset New()
        {
            return new SoundAsset { CompressionRatio = 15, SampleRate = 44100, Spatialized = true, StreamFromDisk = false };
        }
    }

    public class DefaultSoundFactory : AssetFactory<SoundAsset>
    {
        public override SoundAsset New()
        {
            return new SoundAsset { CompressionRatio = 15, SampleRate = 44100, Spatialized = false, StreamFromDisk = false };
        }
    }
}
