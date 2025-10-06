using System;
using System.IO;
using NUnit.Framework;
using TinyWalnutGames.StoryTest.Shared;
using UnityEngine;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Tests for StoryTestSettings configuration system.
    /// </summary>
    public class StoryTestSettingsTests
    {
        [Test]
        public void StoryTestSettings_Instance_ReturnsNonNull()
        {
            var settings = StoryTestSettings.Instance;

            Assert.IsNotNull(settings, "Settings instance should never be null");
        }

        [Test]
        public void StoryTestSettings_HasDefaultValues()
        {
            var settings = StoryTestSettings.Instance;

            Assert.IsNotNull(settings.projectName);
            Assert.IsNotNull(settings.menuPath);
            Assert.IsNotNull(settings.exportPath);
            Assert.IsNotNull(settings.assemblyFilters);
        }

        [Test]
        public void StoryTestSettings_ConceptualValidation_IsInitialized()
        {
            var settings = StoryTestSettings.Instance;

            Assert.IsNotNull(settings.conceptualValidation);
            Assert.IsNotNull(settings.conceptualValidation.validationTiers);
            Assert.IsNotNull(settings.conceptualValidation.environmentCapabilities);
            Assert.IsNotNull(settings.conceptualValidation.customComponentTypes);
            Assert.IsNotNull(settings.conceptualValidation.enumValidationPatterns);
        }

        [Test]
        public void StoryTestSettings_CanReload()
        {
            var settings1 = StoryTestSettings.Instance;

            StoryTestSettings.ReloadSettings();

            var settings2 = StoryTestSettings.Instance;

            Assert.IsNotNull(settings2);
            // Settings should still be functional after reload
            Assert.IsNotNull(settings2.projectName);
        }

        [Test]
        public void StoryTestSettings_SaveAndLoad_Roundtrip()
        {
            var settings = StoryTestSettings.Instance;
            var originalName = settings.projectName;

            // Modify and save
            settings.projectName = "TestProject_" + Guid.NewGuid();
            settings.SaveSettings();

            // Reload
            StoryTestSettings.ReloadSettings();
            var reloadedSettings = StoryTestSettings.Instance;

            // Verify the change persisted
            Assert.AreEqual(settings.projectName, reloadedSettings.projectName);

            // Restore original
            reloadedSettings.projectName = originalName;
            reloadedSettings.SaveSettings();
        }

        [Test]
        public void ValidationTiers_DefaultsCorrectly()
        {
            var tiers = new ValidationTiers();

            // Defaults should be sensible
            Assert.IsTrue(tiers.universal, "Universal validation should be enabled by default");
        }

        [Test]
        public void EnvironmentCapabilities_CanBeConfigured()
        {
            var env = new EnvironmentCapabilities
            {
                hasUnityEngine = true,
                hasDots = false,
                hasBurst = true,
                hasEntities = false,
                canInstantiateComponents = true
            };

            Assert.IsTrue(env.hasUnityEngine);
            Assert.IsFalse(env.hasDots);
            Assert.IsTrue(env.hasBurst);
            Assert.IsFalse(env.hasEntities);
            Assert.IsTrue(env.canInstantiateComponents);
        }

        [Test]
        public void ConceptualValidationConfig_HasReasonableDefaults()
        {
            var config = new ConceptualValidationConfig();

            Assert.IsNotNull(config.validationTiers);
            Assert.IsNotNull(config.environmentCapabilities);
            Assert.IsNotNull(config.customComponentTypes);
            Assert.IsNotNull(config.enumValidationPatterns);
            Assert.IsNotNull(config.fallbackMode);
        }
    }
}
