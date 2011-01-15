using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndAnd;
using NUnit.Framework;

/*
 * NOTE:
 * This test suite comprises just about every use case that works.
 * Nested lambda expressions and the such aren't supported,
 * nor do they really make much sense to implement.
 * Another thing that is not supported is null checking with binary operations.
 */

namespace AndAnd.Tests
{
    [TestFixture]
    public class AndAndExtensionTests
    {
        [Test]
        public void Should_handle_null_parameter()
        {
            Thing thing = null;

            var result = thing.AndAnd(t => t.OtherThing);

            Assert.That(result == null);
        }

        [Test]
        public void Should_handle_null_fields()
        {
            var thing = new Thing();

            var result = thing.AndAnd(t => t.OtherThingField.OtherThing);

            Assert.That(result == null);
        }

        [Test]
        public void Should_handle_null_method_result()
        {
            var thing = new Thing();

            var result = thing.AndAnd(t => t.GetOtherThing().OtherThing);

            Assert.That(result == null);
        }

        [Test]
        public void Should_handle_null_property()
        {
            var thing = new Thing();

            var result = thing.AndAnd(t => t.OtherThing.OtherThing);

            Assert.That(result == null);
        }

        [Test]
        public void Should_properly_return_on_property()
        {
            var thing = new Thing(numOtherThings: 2);

            var result = thing.AndAnd(t => t.OtherThing.OtherThing);

            Assert.That(result == thing.OtherThing.OtherThing);
        }

        [Test]
        public void Should_properly_return_on_method()
        {
            var thing = new Thing(numOtherThings: 2);

            var result = thing.AndAnd(t => t.OtherThing.GetOtherThing());

            Assert.That(result == thing.OtherThing.GetOtherThing());
        }

        [Test]
        public void Should_properly_return_on_field()
        {
            var thing = new Thing(numOtherThings: 2);

            var result = thing.AndAnd(t => t.OtherThing.OtherThingField);

            Assert.That(result == thing.OtherThing.OtherThingField);
        }
    }

    // This is a fugly class for use the tests above . . .
    // You'll notice it contains publicly accessible fields, methods and properties.
    class Thing
    {
        public int ThingNumber { get; set; }

        public Thing OtherThingField;
        public Thing OtherThing
        {
            get { return OtherThingField; }
        }

        public Thing()
            : this(0)
        {
        }

        public Thing(int numOtherThings) 
            : this(1, numOtherThings)
        {
        }

        private Thing(int thingNumber, int numberOtherThings)
        {
            ThingNumber = thingNumber;
            if (numberOtherThings > 0)
            {
                OtherThingField = new Thing(thingNumber + 1, numberOtherThings - 1);
            }
        }

        public Thing GetOtherThing()
        {
            return OtherThing;
        }
    }
}
