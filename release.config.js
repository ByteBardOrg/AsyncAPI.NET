module.exports = {
  branches: ["v2", "vnext"],
  plugins: [
      "@semantic-release/commit-analyzer",
      "@semantic-release/release-notes-generator", 
     [
       "@semantic-release/changelog",
       {
         "changelogFile": "CHANGELOG.md"
       }
     ],
   ]
}
