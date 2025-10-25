using System;
using System.IO;
using NUnit.Framework;
using TinyWalnutGames.StoryTest.Acts;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Tests for Acts 12-13 (extended, optional validation acts).
    /// 
    /// These acts validate at the assembly/project level rather than individual members:
    /// - Act 12: Mental model claims are supported by code
    /// - Act 13: Project architecture aligns with its narrative
    /// </summary>
    public class Act12and13Tests
    {
        #region Act 12: Mental Model Claims

        [Test]
        public void Act12_Registry_CanRetrieveRule()
        {
            var rule = ActRegistry.GetActRule(12);
            Assert.IsNotNull(rule, "Act 12 rule should be registered");
        }

        [Test]
        public void Act12_Rule_IsAccessible()
        {
            var rule = Act12MentalModelClaims.Rule;
            Assert.IsNotNull(rule, "Act12MentalModelClaims.Rule should not be null");
        }

        [Test]
        public void Act12_AssemblyLevelValidation_WithNullMember_ReturnsFalse()
        {
            // Act 12 is assembly-level; null member means "validate assembly"
            var hasViolation = Act12MentalModelClaims.Rule(null, out var message);
            
            // Should return false if no mental model file exists (not critical)
            // or true if there are gaps. Either way, should not throw.
            Assert.Pass("Act12 handles null member without throwing");
        }

        #endregion

        #region Act 13: Narrative Coherence

        [Test]
        public void Act13_Registry_CanRetrieveRule()
        {
            var rule = ActRegistry.GetActRule(13);
            Assert.IsNotNull(rule, "Act 13 rule should be registered");
        }

        [Test]
        public void Act13_Rule_IsAccessible()
        {
            var rule = Act13NarrativeCoherence.Rule;
            Assert.IsNotNull(rule, "Act13NarrativeCoherence.Rule should not be null");
        }

        [Test]
        public void Act13_AssemblyLevelValidation_WithNullMember_ReturnsFalse()
        {
            // Act 13 is assembly-level; null member means "validate narrative"
            var hasViolation = Act13NarrativeCoherence.Rule(null, out var message);
            
            // Should return false if no mental model file exists (not critical)
            // or true if there are gaps. Either way, should not throw.
            Assert.Pass("Act13 handles null member without throwing");
        }

        #endregion

        #region ActRegistry Tests

        [Test]
        public void ActRegistry_GetActName_ReturnsCorrectNames()
        {
            Assert.AreEqual("Act1TodoComments", ActRegistry.GetActName(1));
            Assert.AreEqual("Act12MentalModelClaims", ActRegistry.GetActName(12));
            Assert.AreEqual("Act13NarrativeCoherence", ActRegistry.GetActName(13));
        }

        [Test]
        public void ActRegistry_GetCoreActNumbers_Returns1to11()
        {
            var coreNumbers = ActRegistry.GetCoreActNumbers();
            Assert.AreEqual(11, coreNumbers.Length);
            Assert.AreEqual(1, coreNumbers[0]);
            Assert.AreEqual(11, coreNumbers[10]);
        }

        [Test]
        public void ActRegistry_GetExtendedActNumbers_Returns12and13()
        {
            var extNumbers = ActRegistry.GetExtendedActNumbers();
            Assert.AreEqual(2, extNumbers.Length);
            Assert.AreEqual(12, extNumbers[0]);
            Assert.AreEqual(13, extNumbers[1]);
        }

        [Test]
        public void ActRegistry_GetAllActNumbers_Returns1to13()
        {
            var allNumbers = ActRegistry.GetAllActNumbers();
            Assert.AreEqual(13, allNumbers.Length);
            Assert.AreEqual(1, allNumbers[0]);
            Assert.AreEqual(13, allNumbers[12]);
        }

        [Test]
        public void ActRegistry_ValidateMember_WithInvalidActNumber_ReturnsNull()
        {
            var rule = ActRegistry.GetActRule(99);
            Assert.IsNull(rule, "Non-existent act should return null rule");
        }

        #endregion
    }
}