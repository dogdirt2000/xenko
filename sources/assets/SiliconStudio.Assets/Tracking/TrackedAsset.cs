using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tracking
{
    /// <summary>
    /// Represents a single asset which has source files currently being tracked for changes.
    /// </summary>
    internal class TrackedAsset : IDisposable
    {
        private readonly AssetSourceTracker tracker;
        private readonly Asset sessionAsset;
        private Dictionary<UFile, bool> sourceFiles = new Dictionary<UFile, bool>();
        private Asset clonedAsset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedAsset"/> class.
        /// </summary>
        /// <param name="tracker">The source tracker managing this object.</param>
        /// <param name="sessionAsset">The actual asset in the current session.</param>
        /// <param name="clonedAsset">A clone of the actual asset. If the actual asset is read-only, it is acceptable to use it instead of a clone.</param>
        public TrackedAsset(AssetSourceTracker tracker, Asset sessionAsset, Asset clonedAsset)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (sessionAsset == null) throw new ArgumentNullException(nameof(sessionAsset));
            this.tracker = tracker;
            this.sessionAsset = sessionAsset;
            this.clonedAsset = clonedAsset;
            UpdateAssetImportPathsTracked(true);
        }

        /// <summary>
        /// Gets the id of this asset.
        /// </summary>
        internal Guid AssetId => sessionAsset.Id;

        /// <inheritdoc/>
        public void Dispose()
        {
            // Track asset import paths
            UpdateAssetImportPathsTracked(false);
        }

        /// <summary>
        /// Notifies this object that the asset has been modified.
        /// </summary>
        /// <remarks>This method will trigger the re-evaluation of properties containing the path to a source file.</remarks>
        public void NotifyAssetChanged()
        {
            clonedAsset = (Asset)AssetCloner.Clone(sessionAsset, AssetClonerFlags.KeepBases);
            UpdateAssetImportPathsTracked(true);
        }

        public bool DependsOnSource(UFile sourceFile)
        {
            bool result;
            sourceFiles.TryGetValue(sourceFile, out result);
            return result;
        }

        private void UpdateAssetImportPathsTracked(bool isTracking)
        {
            if (isTracking)
            {
                var collector = new SourceFilesCollector();
                var newSourceFiles = collector.GetSourceFiles(clonedAsset);
                bool changed = false;
                bool needUpdate = false;
                // Untrack previous paths
                foreach (var sourceFile in sourceFiles)
                {
                    if (!newSourceFiles.ContainsKey(sourceFile.Key))
                    {
                        tracker.UnTrackAssetImportInput(AssetId, sourceFile.Key);
                        needUpdate = needUpdate || sourceFile.Value;
                        changed = true;
                    }
                }

                // Track new paths
                foreach (var sourceFile in newSourceFiles)
                {
                    if (!sourceFiles.ContainsKey(sourceFile.Key))
                    {
                        tracker.TrackAssetImportInput(AssetId, sourceFile.Key);
                        needUpdate = needUpdate || sourceFile.Value;
                        changed = true;
                    }
                }

                sourceFiles = newSourceFiles;

                if (changed)
                {
                    var files = sourceFiles.Where(x => x.Value).Select(x => x.Key).ToList();
                    tracker.SourceFileChanged.Post(new[] { new SourceFileChangedData(SourceFileChangeType.Asset, AssetId, files, needUpdate) });
                }
            }
            else
            {
                foreach (var sourceFile in sourceFiles.Keys)
                {
                    tracker.UnTrackAssetImportInput(AssetId, sourceFile);
                }
            }
        }
    }
}
