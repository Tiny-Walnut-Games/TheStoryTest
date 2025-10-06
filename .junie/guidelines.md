Project-specific development guidelines for TheStoryTest

Audience: Experienced Unity + .NET developer using Rider/Unity Test Framework

1) Build and configuration
- Supported toolchain
  - Unity: 6000.2.6f2 (the repo is configured for this exact Editor version)
  - IDE: JetBrains Rider 2024+ with Unity plugin
  - .NET: The repo contains multiple csproj targets (Editor, Runtime, Tests, Player). Build is driven either by Unity or by MSBuild via the solution (.sln).
- Opening the project
  - Open the folder in Unity 6000.2.6f2 once to let Unity generate Library/obj artifacts and sync assemblies.
  - Open TheStoryTest.sln in Rider. Rider will detect the Unity project and link Editor/Player assemblies.
- Building from IDE
  - Preferred: Build from Unity (File → Build) or trigger scripts recompile to produce Editor/Runtime assemblies.
  - From Rider/MSBuild: Build TheStoryTest.sln. If the Shared DLL is locked by Unity, either: close the Unity editor, or build with a different configuration to avoid Debug/obj contention. In practice, building the tests via Rider’s unit test runner is reliable as it triggers the necessary project builds.
- Assembly/asmdef layout (relevant to StoryTest)
  - TinyWalnutGames.TheStoryTest.* projects split code between Runtime (Shared/Player) and Editor.
  - Tests live in TinyWalnutGames.TheStoryTest.Tests(.Player) and under Packages/com.tinywalnutgames.storytest/Tests.
  - StoryTest rule discovery is automatic: StoryIntegrityValidator auto-registers Acts from the Acts assembly at runtime. You typically do not need to wire rules manually.
- Scripting symbols/domain reload
  - Tests and rule discovery can be sensitive to domain reload behavior in Unity. If a test appears to pass in CLI but fail in playmode, re-run after a domain reload and ensure the Acts assembly was loaded.
- Helpful in-repo references
  - QUICKSTART.md – high-level onboarding
  - DYNAMIC_VALIDATION.md – how validation adapts to project structure
  - ASSEMBLY_STRUCTURE.md – how code is partitioned across assemblies
  - PACKAGE_JSON_GUIDE.md – package/Unity package info

2) Testing: configuring, running, and adding tests
- Frameworks/tools
  - NUnit is used throughout (Unity Test Framework underneath when run inside Unity). Namespaces under TinyWalnutGames.StoryTest.Tests.
- Running tests from CLI (JetBrains toolchain)
  - Run the whole test project:
    - run_test TinyWalnutGames.TheStoryTest.Tests.csproj
  - Run by namespace:
    - run_test fqn:TinyWalnutGames.StoryTest.Tests
  - Run a specific class:
    - run_test fqn:TinyWalnutGames.StoryTest.Tests.StoryTestValidationTests
  - Run a specific test method (depending on runner capabilities you may need to run class/namespace; project-wide run always works):
    - If direct FQN filtering doesn’t detect a single test, run the containing project/class as above.
- Unity Test Runner
  - In Unity: Window → Test Runner → EditMode. The same NUnit tests appear there. Use filters to run by assembly or namespace if needed.
- About conceptual tests in this repo
  - ConceptualValidationTests intentionally validate patterns across whatever types exist in loaded assemblies. Depending on your local content, these may fail (e.g., AllEnumTypesHaveValidValues, ValueTypesHaveValidDefaultConstructors). This is expected until your domain assemblies conform. For targeted verification, run specific classes/namespaces (see above) that are deterministic.
- Adding a new test (example)
  - File location: Packages/com.tinywalnutgames.storytest/Tests
  - Namespace: TinyWalnutGames.StoryTest.Tests
  - Example content:
    - using NUnit.Framework;
    - namespace TinyWalnutGames.StoryTest.Tests
      {
        public class SimpleSanityTests
        {
          [Test]
          public void AlwaysTrue_Passes()
          {
            Assert.IsTrue(true);
          }
        }
      }
  - Verified process we used
    - We added a SimpleSanityTests.cs with the snippet above under Packages/com.tinywalnutgames.storytest/Tests.
    - Ran the test suite via: run_test TinyWalnutGames.TheStoryTest.Tests.csproj
    - Outcome on this environment: 10/12 tests passed, including SimpleSanityTests.AlwaysTrue_Passes. Two conceptual tests failed as they are content-dependent, which is acceptable for this demonstration.
    - We then removed the example file to keep the repo clean. You can recreate it locally when following this process.
- Adding tests that interact with StoryTest
  - Use StoryIgnoreAttribute(reason) to suppress known-irrelevant types. Note: StoryIgnoreAttribute requires a non-empty reason and will throw on empty/whitespace.
  - Validation entry points:
    - StoryIntegrityValidator.ValidateAssemblies(params Assembly[])
    - StoryIntegrityValidator.ValidateType(Type)
  - The validator auto-discovers rule sets from the Acts assembly. Ensure the Acts assembly is referenced/loaded in your test context.

3) Additional development notes
- Code style
  - Default Rider C# conventions are acceptable. Keep public API XML docs where present. Prefer explicit namespaces matching assembly partitioning (TinyWalnutGames.StoryTest.*, .Shared, .Acts, etc.).
  - Unity components: follow standard MonoBehaviour patterns; prefer Create/AddComponent in tests and DestroyImmediate for cleanup (see ProductionExcellenceStoryTest_ValidatesConfiguration).
- Debugging validation failures
  - Many tests log violations via UnityEngine.Debug.LogWarning with details. When a validation returns non-empty results, print them to facilitate triage.
  - For assembly scanning behavior, ensure only relevant assemblies are included (see IsProjectAssembly in ConceptualValidationTests). If scanning is too broad, tighten filters or add StoryIgnore to non-target types.
- Common pitfalls
  - Locked binaries during MSBuild (Shared.dll): close Unity or build using a separate configuration. If you hit a file-lock error, prefer running tests via the project’s test csproj rather than the namespace-only runner.
  - Minimal enums: The Act8 HollowEnums rule expects >= 2 values and disallows placeholder-only enums (e.g., None/Default/TEMP). Adjust enums or exclude via StoryIgnore where appropriate.
  - Abstract member checks: Classes with abstract members must be abstract; make them abstract or provide implementations.

4) Quick commands (as used/verified here)
- Run all tests in the primary test project:
  - run_test TinyWalnutGames.TheStoryTest.Tests.csproj
- Run only the deterministic validation tests (avoid conceptual ones):
  - run_test fqn:TinyWalnutGames.StoryTest.Tests.StoryTestValidationTests
- Create a temporary simple test (see example above) under Packages/com.tinywalnutgames.storytest/Tests, run the project tests, then remove the file to keep the repo clean.

Change management for this guidelines file
- Do not add temporary test files to VCS. If you need to include a worked example for documentation, keep it as a snippet in this file and remove the actual file after verifying locally.
