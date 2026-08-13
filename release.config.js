module.exports = {
  branches: ["v2", "vnext"],
  plugins: [
    "@semantic-release/commit-analyzer",
    "@semantic-release/release-notes-generator",
    [
      "@semantic-release/changelog",
      {
        changelogFile: "CHANGELOG.md",
        changelogTitle: "# Changelog\n\nPackage versions follow this library's semantic versioning and do not correspond directly to AsyncAPI specification versions. For example, package 2.0.0 introduced the AsyncAPI 3.0 object model, while package 3.0.0 later moved V3 document output to AsyncAPI 3.1.0.",
      },
    ],
    [
      "@semantic-release/git",
      {
        assets: ["CHANGELOG.md"],
        message: "chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}",
      },
    ],
  ],
};
