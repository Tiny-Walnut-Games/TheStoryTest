# CI/CD Integration

The Story Test Framework integrates seamlessly with CI/CD pipelines to enforce code quality standards automatically.

## GitHub Actions

### Complete Workflow

```yaml
name: Story Test Validation

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:
    inputs:
      run-windows:
        description: "Run Windows Story Test job"
        required: false
        default: "false"
      run-macos:
        description: "Run macOS Story Test job"
        required: false
        default: "false"

jobs:
  # Quick static analysis (fast feedback)
  quick-validation:
    name: Quick Static Analysis
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.11'
          cache: 'pip'
          cache-dependency-path: 'requirements.txt'

      - name: Install dependencies
        run: pip install -r requirements.txt

      - name: Quick validation
        run: python scripts/story_test_unity_safe.py . --verbose

  # Canonical validation (Linux)
  story-test-linux:
    name: Story Test - Linux (Canonical)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.11'
          cache: 'pip'
          cache-dependency-path: 'requirements.txt'

      - name: Install dependencies
        run: pip install -r requirements.txt

      - name: Run Story Test Validation
        run: python scripts/story_test_unity_safe.py . --fail-on-violations --output report.json

      - name: Upload results
        uses: actions/upload-artifact@v4
        with:
          name: story-test-report-linux
          path: report.json

  # Optional Windows validation
  story-test-windows:
    name: Story Test - Windows (Manual)
    if: github.event_name == 'workflow_dispatch' && github.event.inputs.run-windows == 'true'
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.11'
          cache: 'pip'
          cache-dependency-path: 'requirements.txt'

      - name: Install dependencies
        run: pip install -r requirements.txt

      - name: Run Story Test Validation
        run: python scripts/story_test_unity_safe.py . --fail-on-violations --output report.json

      - name: Upload results
        uses: actions/upload-artifact@v4
        with:
          name: story-test-report-windows
          path: report.json
```

### Setup Requirements

1. **Repository Secrets** (at `https://github.com/your-repo/settings/secrets/actions`):
   - `UNITY_LICENSE` (if building Unity projects)
   - `UNITY_EMAIL`
   - `UNITY_PASSWORD`

2. **Workflow File**: Save as `.github/workflows/story-test.yml`

## Azure DevOps

### Pipeline Definition

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main
    - develop

pr:
  branches:
    include:
    - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UsePythonVersion@0
  inputs:
    versionSpec: '3.11'
  displayName: 'Use Python 3.11'

- script: |
    pip install -r requirements.txt
  displayName: 'Install dependencies'

- script: |
    python scripts/story_test_unity_safe.py . --fail-on-violations --output $(Build.ArtifactStagingDirectory)/story-test-report.json
  displayName: 'Run Story Test Validation'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)'
    artifactName: 'story-test-report'
  displayName: 'Publish validation report'
```

## GitLab CI

### GitLab CI Configuration

```yaml
# .gitlab-ci.yml
stages:
  - validate

story-test:
  stage: validate
  image: python:3.11
  before_script:
    - pip install -r requirements.txt
  script:
    - python scripts/story_test_unity_safe.py . --fail-on-violations --output story-test-report.json
  artifacts:
    reports:
      junit: story-test-report.json
    paths:
      - story-test-report.json
  only:
    - merge_requests
    - main
    - develop
```

## Jenkins

### Jenkinsfile

```groovy
pipeline {
    agent any

    stages {
        stage('Story Test Validation') {
            steps {
                sh 'pip install -r requirements.txt'
                sh 'python scripts/story_test_unity_safe.py . --fail-on-violations --output story-test-report.json'

                archiveArtifacts artifacts: 'story-test-report.json', fingerprint: true
            }
        }
    }

    post {
        always {
            publishHTML([
                allowMissing: false,
                alwaysLinkToLastBuild: true,
                keepAll: true,
                reportDir: '.',
                reportFiles: 'story-test-report.json',
                reportName: 'Story Test Report'
            ])
        }
    }
}
```

## CircleCI

### CircleCI Configuration

```yaml
# .circleci/config.yml
version: 2.1

jobs:
  story-test:
    docker:
      - image: cimg/python:3.11
    steps:
      - checkout
      - run:
          name: Install dependencies
          command: pip install -r requirements.txt
      - run:
          name: Run Story Test Validation
          command: python scripts/story_test_unity_safe.py . --fail-on-violations --output story-test-report.json
      - store_artifacts:
          path: story-test-report.json
          destination: story-test-reports

workflows:
  version: 2
  validate:
    jobs:
      - story-test:
          filters:
            branches:
              only:
                - main
                - develop
```

## Docker Integration

### Dockerfile for Validation

```dockerfile
FROM python:3.11-slim

WORKDIR /app

# Copy requirements and install dependencies
COPY requirements.txt .
RUN pip install -r requirements.txt

# Copy validation scripts
COPY scripts/ ./scripts/

# Copy project to validate
COPY . .

# Run validation
CMD ["python", "scripts/story_test_unity_safe.py", ".", "--fail-on-violations"]
```

### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  story-test:
    build: .
    volumes:
      - ./reports:/app/reports
    command: python scripts/story_test_unity_safe.py . --fail-on-violations --output reports/story-test-report.json
```

## Pre-commit Hooks

### Setup with pre-commit

```yaml
# .pre-commit-config.yaml
repos:
  - repo: local
    hooks:
      - id: story-test
        name: Story Test Validation
        entry: python scripts/story_test_unity_safe.py
        language: system
        args: ['.', '--fail-on-violations']
        pass_filenames: false
        always_run: true
```

### Installation

```bash
# Install pre-commit
pip install pre-commit

# Install the hook
pre-commit install

# Run on all files
pre-commit run --all-files
```

## Git Hooks (Manual)

### Pre-commit Hook

```bash
#!/bin/sh
# .git/hooks/pre-commit

echo "Running Story Test validation..."
python scripts/story_test_unity_safe.py . --fail-on-violations

if [ $? -ne 0 ]; then
    echo "Story Test validation failed. Commit aborted."
    exit 1
fi

echo "Story Test validation passed."
```

## Configuration for CI

### CI-Specific Settings

Create `StoryTestSettings.ci.json`:

```json
{
  "projectName": "YourProject",
  "strictMode": true,
  "validateOnStart": false,
  "phases": {
    "enableStoryIntegrity": true,
    "enableCodeCoverage": false,
    "enableArchitecturalCompliance": false,
    "enableProductionReadiness": true,
    "enableSyncPointPerformance": false
  },
  "performance": {
    "maxConcurrentValidations": 2,
    "timeoutSeconds": 60,
    "enableCaching": false
  },
  "reporting": {
    "includeStackTrace": false,
    "includeFileContext": true,
    "exportFormats": ["json"]
  }
}
```

### Environment-Specific Configuration

```bash
# Use CI-specific settings
export STORY_TEST_SETTINGS_PATH="StoryTestSettings.ci.json"
python scripts/story_test_unity_safe.py . --fail-on-violations
```

## Reporting and Notifications

### Slack Integration

```yaml
- name: Notify Slack on Failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: failure
    text: "Story Test validation failed! Check the logs."
  env:
    SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK }}
```

### Email Notifications

```yaml
- name: Send Email on Failure
  if: failure()
  uses: dawidd6/action-send-mail@v3
  with:
    server_address: smtp.gmail.com
    server_port: 587
    username: ${{ secrets.EMAIL_USERNAME }}
    password: ${{ secrets.EMAIL_PASSWORD }}
    subject: "Story Test Validation Failed"
    body: "Story Test validation failed for commit ${{ github.sha }}"
    to: ${{ secrets.NOTIFICATION_EMAIL }}
```

## Performance Optimization

### Caching Strategy

```yaml
- name: Cache Python Dependencies
  uses: actions/cache@v4
  with:
    path: ~/.cache/pip
    key: ${{ runner.os }}-pip-${{ hashFiles('**/requirements.txt') }}
    restore-keys: |
      ${{ runner.os }}-pip-
```

### Parallel Execution

```yaml
strategy:
  matrix:
    assembly: [Assembly-CSharp, MyGame.Logic, MyGame.Core]

runs-on: ubuntu-latest

steps:
  - name: Validate Assembly
    run: python scripts/story_test_unity_safe.py Library/ScriptAssemblies/${{ matrix.assembly }}.dll --fail-on-violations
```

## Best Practices

### 1. Fast Feedback
- Run quick validation first
- Use parallel execution for multiple assemblies
- Cache dependencies between runs

### 2. Gradual Adoption
- Start with warnings only
- Gradually increase strictness
- Allow temporary exemptions with `[StoryIgnore]`

### 3. Monitoring
- Track validation trends over time
- Alert on increasing violation counts
- Monitor validation performance

### 4. Team Integration
- Provide clear failure messages
- Include fix suggestions in reports
- Document team-specific guidelines

## Troubleshooting

### Common CI Issues

**Python not found**
```yaml
- uses: actions/setup-python@v5
  with:
    python-version: '3.11'
```

**Dependencies not installed**
```yaml
- name: Cache pip dependencies
  uses: actions/cache@v4
  with:
    path: ~/.cache/pip
    key: ${{ runner.os }}-pip-${{ hashFiles('**/requirements.txt') }}
```

**Validation timeout**
```json
{
  "performance": {
    "timeoutSeconds": 120,
    "maxConcurrentValidations": 1
  }
}
```

**Assembly not found**
- Verify Unity build completed successfully
- Check assembly paths in CI environment
- Use Unity-safe validator for cross-platform compatibility

Remember: CI/CD integration should enforce quality without blocking legitimate development progress.