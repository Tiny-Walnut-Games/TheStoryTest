using TinyWalnutGames.StoryTest.Shared;
using TinyWalnutGames.StoryTest.Components;
using TinyWalnutGames.StoryTest;
using NUnit.Framework;
using Unity.Mathematics;
using System.Reflection;
using System.Linq;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Tests for the Story Test validation framework.
    /// Ensures the narrative integrity system works correctly.
    /// </summary>
    public class StoryTestValidationTests
    {
        [Test]
        public void StoryIgnoreAttribute_RequiresReason()
        {
            // Test that StoryIgnoreAttribute requires a non-empty reason
            Assert.Throws<System.ArgumentException>(() => new StoryIgnoreAttribute(""));
            Assert.Throws<System.ArgumentException>(() => new StoryIgnoreAttribute(null));
            Assert.Throws<System.ArgumentException>(() => new StoryIgnoreAttribute("   "));

            // Valid reason should not throw
            Assert.DoesNotThrow(() => new StoryIgnoreAttribute("Valid reason"));
        }

        [Test]
        public void StoryIntegrityValidator_ValidatesAssemblies()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var violations = StoryIntegrityValidator.ValidateAssemblies(assembly);

            // Should return a list (may be empty, but not null)
            Assert.IsNotNull(violations);

            // For a production-ready system, there should be no violations
            if (violations.Count > 0)
            {
                UnityEngine.Debug.LogWarning($"Story violations found: {string.Join(", ", violations.Select(v => v.ToString()))}");
            }
        }

        [Test]
        public void StoryIntegrityValidator_RespectsStoryIgnoreAttribute()
        {
            // Test that StoryIgnore attribute is respected
            var testType = typeof(TestClassWithStoryIgnore);
            var violations = StoryIntegrityValidator.ValidateType(testType);

            // Should have no violations since class is marked with StoryIgnore
            Assert.AreEqual(0, violations.Count,
                $"Expected no violations for StoryIgnore class, but found: {string.Join(", ", violations.Select(v => v.ToString()))}");
        }

        [Test]
        public void ProductionExcellenceStoryTest_ValidatesConfiguration()
        {
            var testObject = new UnityEngine.GameObject("Test Story Test");
            var storyTest = testObject.AddComponent<ProductionExcellenceStoryTest>();

            // Test that component can be created without errors
            Assert.IsNotNull(storyTest);

            // Cleanup
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    /// <summary>
    /// Tests for core DOTS components to ensure they work correctly.
    /// </summary>
    public class CoreComponentTests
    {
        [Test]
        public void Position_ConstructorsWork()
        {
            var pos1 = new Position(new float2(5, 10));
            Assert.AreEqual(new float2(5, 10), pos1.Value);
            Assert.AreEqual(int2.zero, pos1.WorldSection);

            var pos2 = new Position(new float2(3, 7), new int2(1, 2));
            Assert.AreEqual(new float2(3, 7), pos2.Value);
            Assert.AreEqual(new int2(1, 2), pos2.WorldSection);
        }

        [Test]
        public void Position_GetTrueWorldPosition()
        {
            var position = new Position(new float2(5, 10), new int2(2, 3));
            var worldSize = new float2(100, 200);

            var truePosition = position.GetTrueWorldPosition(worldSize);
            var expected = new float2(5 + 2 * 100, 10 + 3 * 200);

            Assert.AreEqual(expected, truePosition);
        }

        [Test]
        public void Health_TakeDamageWorks()
        {
            var health = new Health(100f);
            Assert.AreEqual(100f, health.Current);
            Assert.AreEqual(100f, health.Max);
            Assert.IsFalse(health.IsDead);

            health.TakeDamage(30f, 1f);
            Assert.AreEqual(70f, health.Current);
            Assert.IsFalse(health.IsDead);

            health.TakeDamage(80f, 2f);
            Assert.AreEqual(0f, health.Current);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(2f, health.DeathTime);
        }

        [Test]
        public void Health_HealWorks()
        {
            var health = new Health(100f);
            health.TakeDamage(50f, 1f);

            health.Heal(20f);
            Assert.AreEqual(70f, health.Current);

            health.Heal(50f); // Should clamp to max
            Assert.AreEqual(100f, health.Current);
        }

        [Test]
        public void Health_CannotHealDead()
        {
            var health = new Health(100f);
            health.TakeDamage(100f, 1f);
            Assert.IsTrue(health.IsDead);

            health.Heal(50f);
            Assert.AreEqual(0f, health.Current);
            Assert.IsTrue(health.IsDead);
        }
    }

    /// <summary>
    /// Tests for the biome and world wrapping systems.
    /// </summary>
    public class BiomeSystemTests
    {
        [Test]
        public void BiomeContext_InitializesCorrectly()
        {
            var biomeContext = new BiomeContext
            {
                CurrentBiome = 1,
                PreviousBiome = 0,
                IsTransitioning = false,
                TransitionProgress = 0f
            };

            Assert.AreEqual(1, biomeContext.CurrentBiome);
            Assert.AreEqual(0, biomeContext.PreviousBiome);
            Assert.IsFalse(biomeContext.IsTransitioning);
            Assert.AreEqual(0f, biomeContext.TransitionProgress);
        }

        [Test]
        public void WorldBounds_ConfigurationIsValid()
        {
            var bounds = new WorldBounds
            {
                Size = new float2(200, 300),
                Center = new float2(10, 20),
                NormalizationThreshold = 1000f
            };

            Assert.AreEqual(new float2(200, 300), bounds.Size);
            Assert.AreEqual(new float2(10, 20), bounds.Center);
            Assert.AreEqual(1000f, bounds.NormalizationThreshold);
        }
    }

    /// <summary>
    /// Tests for pickup and weapon systems.
    /// </summary>
    public class GameplaySystemTests
    {
        [Test]
        public void Pickup_ConfigurationIsValid()
        {
            var pickup = new Pickup
            {
                Type = PickupType.Health,
                Value = 25f,
                CollectionRadius = 1.5f,
                IsCollected = false
            };

            Assert.AreEqual(PickupType.Health, pickup.Type);
            Assert.AreEqual(25f, pickup.Value);
            Assert.AreEqual(1.5f, pickup.CollectionRadius);
            Assert.IsFalse(pickup.IsCollected);
        }

        [Test]
        public void Enemy_ConfigurationIsValid()
        {
            var enemy = new Enemy
            {
                Type = EnemyType.ToxicZombie,
                Damage = 35f,
                AttackRate = 0.5f,
                AggroRange = 8f,
                ExperienceValue = 15f,
                LastAttackTime = 0f
            };

            Assert.AreEqual(EnemyType.ToxicZombie, enemy.Type);
            Assert.AreEqual(35f, enemy.Damage);
            Assert.AreEqual(0.5f, enemy.AttackRate);
            Assert.AreEqual(8f, enemy.AggroRange);
            Assert.AreEqual(15f, enemy.ExperienceValue);
        }

        [Test]
        public void Projectile_ConfigurationIsValid()
        {
            var projectile = new Projectile
            {
                Damage = 50f,
                MaxRange = 15f,
                Speed = 10f,
                Direction = math.normalize(new float2(1, 1)),
                TraveledDistance = 0f
            };

            Assert.AreEqual(50f, projectile.Damage);
            Assert.AreEqual(15f, projectile.MaxRange);
            Assert.AreEqual(10f, projectile.Speed);
            Assert.IsTrue(math.length(projectile.Direction) > 0.9f); // Should be normalized
        }

        [Test]
        public void ToxicEmitter_ConfigurationIsValid()
        {
            var emitter = new ToxicEmitter
            {
                EffectRadius = 3f,
                DamagePerSecond = 10f,
                Duration = 5f,
                RemainingDuration = 5f,
                AffectsAll = false
            };

            Assert.AreEqual(3f, emitter.EffectRadius);
            Assert.AreEqual(10f, emitter.DamagePerSecond);
            Assert.AreEqual(5f, emitter.Duration);
            Assert.AreEqual(5f, emitter.RemainingDuration);
            Assert.IsFalse(emitter.AffectsAll);
        }
    }

    /// <summary>
    /// Integration tests for the complete system.
    /// </summary>
    public class IntegrationTests
    {
        [Test]
        public void AllEnumTypesHaveValidValues()
        {
            // Test that all enum types have at least one valid value
            var enemyTypes = System.Enum.GetValues(typeof(EnemyType));
            Assert.IsTrue(enemyTypes.Length > 0, "EnemyType enum should have values");

            var pickupTypes = System.Enum.GetValues(typeof(PickupType));
            Assert.IsTrue(pickupTypes.Length > 0, "PickupType enum should have values");

            var weaponTypes = System.Enum.GetValues(typeof(WeaponType));
            Assert.IsTrue(weaponTypes.Length > 0, "WeaponType enum should have values");
        }

        [Test]
        public void ComponentsHaveValidDefaultValues()
        {
            // Test that components can be created with default values
            var movement = new Movement();
            Assert.AreEqual(float2.zero, movement.Velocity);

            var playerInput = new PlayerInput();
            Assert.AreEqual(float2.zero, playerInput.MovementInput);
            Assert.IsFalse(playerInput.PrimaryFire);

            var player = new Player();
            Assert.AreEqual(0, player.Level);
            Assert.AreEqual(0f, player.Experience);
        }

        [Test]
        public void StoryTestCompliance_NoViolationsInCoreComponents()
        {
            // Validate that core component types follow story test rules
            var componentTypes = new[]
            {
                typeof(Position),
                typeof(Movement),
                typeof(Health),
                typeof(Player),
                typeof(Enemy),
                typeof(Pickup),
                typeof(Projectile)
            };

            foreach (var type in componentTypes)
            {
                var violations = StoryIntegrityValidator.ValidateType(type);

                if (violations.Count > 0)
                {
                    var violationMessages = string.Join(", ", violations.Select(v => v.ToString()));
                    UnityEngine.Debug.LogWarning($"Component {type.Name} has story violations: {violationMessages}");
                }

                // For production readiness, we expect zero violations
                // Comment out the assertion below if you need to allow some violations during development
                Assert.AreEqual(0, violations.Count,
                    $"Component {type.Name} should have no story violations, but found: {string.Join(", ", violations.Select(v => v.ToString()))}");
            }
        }
    }

    /// <summary>
    /// Test class marked with StoryIgnore for testing purposes.
    /// </summary>
        [TinyWalnutGames.StoryTest.Shared.StoryIgnoreAttribute("Test class for validating StoryIgnore attribute functionality")]
    public class TestClassWithStoryIgnore
    {
        public void SomeMethod()
        {
            // This would normally be flagged as incomplete, but StoryIgnore should prevent it
        }
    }
}