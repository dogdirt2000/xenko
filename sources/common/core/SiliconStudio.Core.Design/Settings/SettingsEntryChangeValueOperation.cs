﻿using SiliconStudio.Core.Transactions;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal object allowing to rollback/rollforward changes in a <see cref="SettingsEntry"/>.
    /// </summary>
    internal sealed class SettingsEntryChangeValueOperation : Operation
    {
        private readonly SettingsEntry entry;
        private object oldValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEntryChangeValueOperation"/> class.
        /// </summary>
        /// <param name="entry">The settings entry that has been modified.</param>
        /// <param name="oldValue">The value of the settings entry before the modification.</param>
        public SettingsEntryChangeValueOperation(SettingsEntry entry, object oldValue)
        {
            this.entry = entry;
            this.oldValue = oldValue;
        }

        /// <inheritdoc/>
        protected override void Rollback()
        {
            var newValue = oldValue;
            oldValue = entry.Value;
            entry.Value = newValue;
        }

        /// <inheritdoc/>
        protected override void Rollforward()
        {
            Rollback();
        }
    }
}