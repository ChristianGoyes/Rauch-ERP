const tseslint = require('typescript-eslint');
const angular = require('@angular-eslint/eslint-plugin');
const angularTemplate = require('@angular-eslint/eslint-plugin-template');
const templateParser = require('@angular-eslint/template-parser');

module.exports = tseslint.config(
  { ignores: ['dist/**', '.angular/**', 'node_modules/**', 'playwright-report/**', 'eslint.config.js'] },
  ...tseslint.configs.recommended,
  { files: ['**/*.ts'], plugins: { '@angular-eslint': angular, '@angular-eslint/template': angularTemplate }, processor: angularTemplate.processors['extract-inline-html'], rules: { '@typescript-eslint/no-explicit-any': 'error' } },
  { files: ['**/*.html'], languageOptions: { parser: templateParser }, plugins: { '@angular-eslint/template': angularTemplate }, rules: {} }
);
