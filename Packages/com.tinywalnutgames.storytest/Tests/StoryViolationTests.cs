using System;
using NUnit.Framework;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Tests for StoryViolation data structure and utilities.
    /// </summary>
    public class StoryViolationTests
    {
        [Test]
        public void StoryViolation_CanBeCreated()
        {
            var violation = new StoryViolation
            {
                Type = "TestType",
                Member = "TestMember",
                Violation = "Test violation message",
                FilePath = "TestFile.cs",
                LineNumber = 42,
                ViolationType = StoryViolationType.IncompleteImplementation
            };

            Assert.AreEqual("TestType", violation.Type);
            Assert.AreEqual("TestMember", violation.Member);
            Assert.AreEqual("Test violation message", violation.Violation);
            Assert.AreEqual("TestFile.cs", violation.FilePath);
            Assert.AreEqual(42, violation.LineNumber);
            Assert.AreEqual(StoryViolationType.IncompleteImplementation, violation.ViolationType);
        }

        [Test]
        public void StoryViolation_ToString_FormatsCorrectly()
        {
            var violation = new StoryViolation
            {
                Type = "TestType",
                Member = "TestMember",
                Violation = "Test violation",
                ViolationType = StoryViolationType.IncompleteImplementation
            };

            var str = violation.ToString();

            Assert.IsNotNull(str);
            Assert.IsTrue(str.Contains("TestType"), "Should contain type name");
            Assert.IsTrue(str.Contains("TestMember"), "Should contain member name");
        }

        [Test]
        public void StoryViolationType_AllValuesAreDefined()
        {
            // Verify all enum values are distinct and defined
            var values = Enum.GetValues(typeof(StoryViolationType));

            Assert.Greater(values.Length, 0, "StoryViolationType should have at least one value");

            // Verify each value can be converted to string
            foreach (StoryViolationType violationType in values)
            {
                var name = violationType.ToString();
                Assert.IsNotEmpty(name, $"Enum value {violationType} should have a name");
            }
        }

        [Test]
        public void StoryTestUtilities_GetViolationType_HandlesKnownPatterns()
        {
            // Test known violation patterns
            Assert.AreEqual(StoryViolationType.IncompleteImplementation,
                StoryTestUtilities.GetViolationType("TODO implementation"));

            Assert.AreEqual(StoryViolationType.IncompleteImplementation,
                StoryTestUtilities.GetViolationType("NotImplementedException"));

            Assert.AreEqual(StoryViolationType.PlaceholderCode,
                StoryTestUtilities.GetViolationType("placeholder"));

            Assert.AreEqual(StoryViolationType.NamingConvention,
                StoryTestUtilities.GetViolationType("naming convention"));
        }

        [Test]
        public void StoryTestUtilities_GetViolationType_ReturnsOtherForUnknown()
        {
            var result = StoryTestUtilities.GetViolationType("some random unknown violation text");

            Assert.AreEqual(StoryViolationType.Other, result,
                "Unknown violation patterns should return 'Other'");
        }

        [Test]
        public void StoryViolation_CanBeUsedInCollections()
        {
            var violations = new System.Collections.Generic.List<StoryViolation>
            {
                new StoryViolation { Type = "Type1", Member = "Member1", Violation = "Violation1" },
                new StoryViolation { Type = "Type2", Member = "Member2", Violation = "Violation2" },
                new StoryViolation { Type = "Type3", Member = "Member3", Violation = "Violation3" }
            };

            Assert.AreEqual(3, violations.Count);
            Assert.AreEqual("Type1", violations[0].Type);
            Assert.AreEqual("Member2", violations[1].Member);
            Assert.AreEqual("Violation3", violations[2].Violation);
        }

        [Test]
        public void StoryViolation_DefaultValues_AreReasonable()
        {
            var violation = new StoryViolation();

            // Default values should not cause crashes
            Assert.DoesNotThrow(() => violation.ToString());
            Assert.AreEqual(0, violation.LineNumber);
            Assert.AreEqual(StoryViolationType.Other, violation.ViolationType);
        }
    }
}
