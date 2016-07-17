using NUnit.Framework;

namespace SiliconStudio.Core.Design.Tests.Transactions
{
    internal class OrderedOperation : SimpleOperation
    {
        private readonly Counter counter;
        private readonly int order;
        private readonly int totalCount;

        internal class Counter
        {
            public void Reset() => Value = 0;
            public int Value { get; set; }
        }

        public OrderedOperation(Counter counter, int order, int totalCount)
        {
            this.counter = counter;
            this.order = order;
            this.totalCount = totalCount;
        }

        protected override void Rollback()
        {
            // Rollback is done in reverse order
            var value = totalCount - order - 1;
            Assert.AreEqual(value, counter.Value);
            counter.Value++;
            base.Rollback();
        }

        protected override void Rollforward()
        {
            Assert.AreEqual(order, counter.Value);
            counter.Value++;
            base.Rollforward();
        }
    }
}
