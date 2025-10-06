using System;
using System.Linq;
using NUnit.Framework;
using TinyWalnutGames.StoryTest;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Tests for ValidationReport functionality.
    /// </summary>
    public class ValidationReportTests
    {
        [Test]
        public void ValidationReport_InitializesCorrectly()
        {
            var report = new ValidationReport();

            Assert.IsNotNull(report);
            Assert.IsNotNull(report.StoryViolations);
            Assert.IsNotNull(report.PhaseViolations);
            Assert.IsNotNull(report.PhaseNotes);
            Assert.AreEqual(0, report.StoryViolations.Count);
        }

        [Test]
        public void ValidationReport_AddViolations_TracksByPhase()
        {
            var report = new ValidationReport();
            var violations = new[]
            {
                new StoryViolation
                {
                    Type = "TestType",
                    Member = "TestMember",
                    Violation = "Test violation",
                    ViolationType = StoryViolationType.IncompleteImplementation
                }
            };

            report.AddViolations("TestPhase", violations);

            Assert.AreEqual(1, report.StoryViolations.Count);
            Assert.IsTrue(report.PhaseViolations.ContainsKey("TestPhase"));
            Assert.AreEqual(1, report.PhaseViolations["TestPhase"].Count);
        }

        [Test]
        public void ValidationReport_AddNote_StoresNoteByPhase()
        {
            var report = new ValidationReport();

            report.AddNote("TestPhase", "Test note");

            Assert.IsTrue(report.PhaseNotes.ContainsKey("TestPhase"));
            Assert.AreEqual(1, report.PhaseNotes["TestPhase"].Count);
            Assert.AreEqual("Test note", report.PhaseNotes["TestPhase"][0]);
        }

        [Test]
        public void ValidationReport_IsFullyCompliant_WhenNoViolations()
        {
            var report = new ValidationReport();

            Assert.IsTrue(report.IsFullyCompliant);
        }

        [Test]
        public void ValidationReport_IsNotCompliant_WhenHasViolations()
        {
            var report = new ValidationReport();
            report.AddViolations("Test", new[]
            {
                new StoryViolation
                {
                    Type = "Test",
                    Member = "Test",
                    Violation = "Test",
                    ViolationType = StoryViolationType.IncompleteImplementation
                }
            });

            Assert.IsFalse(report.IsFullyCompliant);
        }

        [Test]
        public void ValidationReport_ProductionReadinessScore_Decreases()
        {
            var report = new ValidationReport();
            var initialScore = report.ProductionReadinessScore;

            Assert.AreEqual(100f, initialScore);

            report.AddViolations("Test", new[]
            {
                new StoryViolation
                {
                    Type = "Test",
                    Member = "Test",
                    Violation = "Test",
                    ViolationType = StoryViolationType.IncompleteImplementation
                }
            });

            Assert.Less(report.ProductionReadinessScore, 100f);
        }

        [Test]
        public void ValidationReport_GenerateSummary_ContainsKeyInfo()
        {
            var report = new ValidationReport
            {
                StartedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow.AddSeconds(1),
                Duration = TimeSpan.FromSeconds(1)
            };

            report.AddViolations("TestPhase", new[]
            {
                new StoryViolation
                {
                    Type = "TestType",
                    Member = "TestMember",
                    Violation = "Test violation",
                    ViolationType = StoryViolationType.IncompleteImplementation
                }
            });

            var summary = report.GenerateSummary();

            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.Contains("TestPhase"), "Summary should contain phase name");
            Assert.IsTrue(summary.Contains("TestType"), "Summary should contain type name");
            Assert.IsTrue(summary.Contains("violation"), "Summary should contain violation info");
        }

        [Test]
        public void ValidationReport_ShouldStop_ReturnsTrueWhenStopOnFirstAndHasViolation()
        {
            var report = new ValidationReport();
            report.AddViolations("Test", new[]
            {
                new StoryViolation
                {
                    Type = "Test",
                    Member = "Test",
                    Violation = "Test",
                    ViolationType = StoryViolationType.IncompleteImplementation
                }
            });

            Assert.IsTrue(report.ShouldStop(stopOnFirstViolation: true));
            Assert.IsFalse(report.ShouldStop(stopOnFirstViolation: false));
        }
    }
}
