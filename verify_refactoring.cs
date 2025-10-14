// Quick verification script for AdvancedILAnalysis refactoring
// This is a temporary file to verify the refactoring works correctly

using System;
using System.Reflection;

namespace TinyWalnutGames.StoryTest.Verification
{
    class VerifyRefactoring
    {
        static void Main()
        {
            Console.WriteLine("Verifying AdvancedILAnalysis refactoring...");
            
            // Test 1: Verify ShouldSkipType still works
            var testType = typeof(VerifyRefactoring);
            var result1 = TinyWalnutGames.StoryTest.Shared.AdvancedILAnalysis.ShouldSkipType(testType);
            Console.WriteLine($"ShouldSkipType(VerifyRefactoring): {result1}");
            
            // Test 2: Verify ShouldSkipMember still works
            var method = testType.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            var result2 = TinyWalnutGames.StoryTest.Shared.AdvancedILAnalysis.ShouldSkipMember(method);
            Console.WriteLine($"ShouldSkipMember(Main): {result2}");
            
            // Test 3: Verify null handling
            var result3 = TinyWalnutGames.StoryTest.Shared.AdvancedILAnalysis.ShouldSkipType(null);
            Console.WriteLine($"ShouldSkipType(null): {result3}");
            
            var result4 = TinyWalnutGames.StoryTest.Shared.AdvancedILAnalysis.ShouldSkipMember(null);
            Console.WriteLine($"ShouldSkipMember(null): {result4}");
            
            Console.WriteLine("Verification complete!");
        }
    }
}