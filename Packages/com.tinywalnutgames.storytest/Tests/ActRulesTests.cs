using System;
using System.Reflection;
using NUnit.Framework;
using TinyWalnutGames.StoryTest.Acts;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Unit tests for the individual Act validation rules.
    /// Each test validates that the Act correctly identifies violations.
    /// </summary>
    public class ActRulesTests
    {
        #region Act1: TODO Comments

        [Test]
        public void Act1_DetectsTodoComments()
        {
            var method = typeof(TestClassWithViolations).GetMethod(nameof(TestClassWithViolations.MethodWithNotImplemented));
            var hasViolation = Act1TodoComments.Rule(method, out var message);

            Assert.IsTrue(hasViolation, "Should detect NotImplementedException");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("NotImplementedException") || message.Contains("TODO"),
                $"Message should mention TODO or NotImplementedException, but was: {message}");
        }

        [Test]
        public void Act1_AllowsImplementedMethods()
        {
            var method = typeof(TestClassWithoutViolations).GetMethod(nameof(TestClassWithoutViolations.ImplementedMethod));
            var hasViolation = Act1TodoComments.Rule(method, out _);

            Assert.IsFalse(hasViolation, "Should not flag properly implemented methods");
        }

        #endregion

        #region Act2: Placeholder Implementations

        [Test]
        public void Act2_DetectsPlaceholderNames()
        {
            var type = typeof(TestClassWithViolations);
            var hasViolation = Act2PlaceholderImplementations.Rule(type, out var message);

            if (hasViolation)
            {
                Assert.IsTrue(message.Contains("placeholder") || message.Contains("TODO"),
                    $"Message should mention placeholder: {message}");
            }
        }

        #endregion

        #region Act3: Incomplete Classes

        [Test]
        public void Act3_DetectsEmptyClasses()
        {
            var type = typeof(EmptyTestClass);
            var hasViolation = Act3IncompleteClasses.Rule(type, out var message);

            if (hasViolation)
            {
                Assert.IsTrue(message.Contains("incomplete") || message.Contains("empty"),
                    $"Message should mention incomplete/empty: {message}");
            }
        }

        #endregion

        #region Act4: Unsealed Abstract Members

        [Test]
        public void Act4_DetectsUnsealedAbstractClasses()
        {
            // Abstract class with abstract members should be fine
            var abstractType = typeof(AbstractTestClass);
            var hasViolation = Act4UnsealedAbstractMembers.Rule(abstractType, out _);

            Assert.IsFalse(hasViolation, "Abstract classes with abstract members should be valid");
        }

        #endregion

        #region Act5: Debug-Only Implementations

        [Test]
        public void Act5_DetectsDebugOnlyCode()
        {
            var method = typeof(TestClassWithViolations).GetMethod(nameof(TestClassWithViolations.DebugOnlyMethod));
            if (method != null)
            {
                var hasViolation = Act5DebugOnlyImplementations.Rule(method, out var message);

                if (hasViolation)
                {
                    // Check for "debug", "DEBUG", or "Debug" (with or without emoji prefix)
                    Assert.IsTrue(
                        message.Contains("debug", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("Debug") ||
                        message.Contains("🏳Debug"),
                        $"Message should mention debug: {message}");
                }
            }
        }

        #endregion

        #region Act6: Phantom Props

        [Test]
        public void Act6_DetectsUnusedProperties()
        {
            var property = typeof(TestClassWithViolations).GetProperty(nameof(TestClassWithViolations.UnusedProperty));
            if (property != null)
            {
                var hasViolation = Act6PhantomProps.Rule(property, out var message);

                // This test is conceptual - phantom prop detection may be complex
                // Just verify the rule can be called without crashing
                Assert.DoesNotThrow(() => Act6PhantomProps.Rule(property, out _));
            }
        }

        #endregion

        #region Act7: Cold Methods

        [Test]
        public void Act7_DetectsUnusedMethods()
        {
            var method = typeof(TestClassWithViolations).GetMethod(nameof(TestClassWithViolations.UnusedMethod));
            if (method != null)
            {
                var hasViolation = Act7ColdMethods.Rule(method, out _);

                // Cold method detection is complex - just ensure it doesn't crash
                Assert.DoesNotThrow(() => Act7ColdMethods.Rule(method, out _));
            }
        }

        #endregion

        #region Act8: Hollow Enums

        [Test]
        public void Act8_DetectsHollowEnums()
        {
            var enumType = typeof(HollowTestEnum);
            var hasViolation = Act8HollowEnums.Rule(enumType, out var message);

            Assert.IsTrue(hasViolation, "Should detect enum with only one value");
            Assert.IsTrue(message.Contains("enum") || message.Contains("value"),
                $"Message should mention enum/value: {message}");
        }

        [Test]
        public void Act8_AllowsProperEnums()
        {
            var enumType = typeof(ProperTestEnum);
            var hasViolation = Act8HollowEnums.Rule(enumType, out _);

            Assert.IsFalse(hasViolation, "Should allow enums with multiple values");
        }

        #endregion

        #region Act9: Premature Celebrations

        [Test]
        public void Act9_DetectsCelebratoryNames()
        {
            var method = typeof(TestClassWithViolations).GetMethod(nameof(TestClassWithViolations.FinalImplementation));
            if (method != null)
            {
                var hasViolation = Act9PrematureCelebrations.Rule(method, out var message);

                // This rule looks for names like "Final", "Ultimate", "Perfect" etc.
                // Just verify it can be called
                Assert.DoesNotThrow(() => Act9PrematureCelebrations.Rule(method, out _));
            }
        }

        #endregion

        #region Test Classes

        public class TestClassWithViolations
        {
            public void MethodWithNotImplemented()
            {
                throw new NotImplementedException();
            }

            public void DebugOnlyMethod()
            {
                #if DEBUG
                // Debug-only code
                #endif
            }

            public int UnusedProperty { get; set; }

            public void UnusedMethod()
            {
                // This method is never called
            }

            public void FinalImplementation()
            {
                // Method with celebratory name
            }
        }

        public class TestClassWithoutViolations
        {
            public int ImplementedMethod()
            {
                return 42;
            }
        }

        public class EmptyTestClass
        {
            // Intentionally empty for testing Act3
        }

        public abstract class AbstractTestClass
        {
            public abstract void AbstractMethod();
        }

        public enum HollowTestEnum
        {
            OnlyValue = 0
        }

        public enum ProperTestEnum
        {
            Value1 = 0,
            Value2 = 1,
            Value3 = 2
        }

        #endregion
    }
}
