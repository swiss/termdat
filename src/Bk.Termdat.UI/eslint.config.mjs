import angularPlugin from '@angular-eslint/eslint-plugin';
import angularTemplatePlugin from '@angular-eslint/eslint-plugin-template';
import angularTemplateParser from '@angular-eslint/template-parser';
import tsPlugin from '@typescript-eslint/eslint-plugin';
import {defineConfig, globalIgnores} from 'eslint/config';
import prettierConfig from 'eslint-config-prettier';
import prettierPlugin from 'eslint-plugin-prettier';
import importPlugin from 'eslint-plugin-import';

export default defineConfig([
    globalIgnores(['projects/**/coverage/*', 'coverage/*', '**/tests']),

    // typescript-eslint flat/recommended (parser, eslint overrides, ts rules)
    ...tsPlugin.configs['flat/recommended'].map(cfg => ({
        ...cfg,
        files: ['**/*.ts'],
    })),

    // Add project-aware parserOptions for type-checked rules
    {
        files: ['**/*.ts'],
        languageOptions: {
            parserOptions: {
                ecmaFeatures: {modules: true},
                project: ['tsconfig.eslint.json'],
            },
        },
    },

    // Angular, import, prettier plugins + all custom rules
    {
        files: ['**/*.ts'],
        plugins: {
            '@angular-eslint': angularPlugin,
            import: importPlugin,
            prettier: prettierPlugin,
        },
        processor: angularTemplatePlugin.processors['extract-inline-html'],
        rules: {
            ...angularPlugin.configs.recommended.rules,
            ...prettierConfig.rules,
            'prettier/prettier': 'error',

            curly: 'error',
            '@typescript-eslint/member-ordering': [
                'error',
                {
                    default: [
                        'public-static-field',
                        'public-instance-field',
                        'protected-static-field',
                        'protected-instance-field',
                        'private-static-field',
                        'private-instance-field',
                        'public-constructor',
                        'protected-constructor',
                        'private-constructor',
                        'public-static-method',
                        'public-instance-method',
                        'protected-static-method',
                        'protected-instance-method',
                        'private-static-method',
                        'private-instance-method',
                    ],
                },
            ],

            '@angular-eslint/no-pipe-impure': 'error',
            '@angular-eslint/prefer-inject': 'off',
            '@angular-eslint/prefer-standalone': 'error',
            '@angular-eslint/prefer-output-readonly': 'error',
            '@angular-eslint/relative-url-prefix': 'error',
            '@angular-eslint/use-component-selector': 'error',
            '@angular-eslint/no-empty-lifecycle-method': 'off',
            '@typescript-eslint/array-type': 'error',
            '@typescript-eslint/ban-tslint-comment': 'error',
            '@typescript-eslint/consistent-type-assertions': 'error',
            '@typescript-eslint/default-param-last': 'error',
            '@typescript-eslint/method-signature-style': 'error',
            '@typescript-eslint/no-floating-promises': 'error',
            '@typescript-eslint/no-unnecessary-boolean-literal-compare': 'error',
            '@typescript-eslint/no-unnecessary-type-assertion': 'error',
            '@typescript-eslint/prefer-includes': 'error',
            '@typescript-eslint/prefer-optional-chain': 'error',
            '@typescript-eslint/prefer-readonly': 'error',
            '@typescript-eslint/prefer-regexp-exec': 'error',
            '@typescript-eslint/prefer-string-starts-ends-with': 'error',
            '@typescript-eslint/unified-signatures': 'error',

            '@typescript-eslint/no-unused-vars': [
                'error',
                {
                    argsIgnorePattern: '^_$',
                },
            ],

            '@typescript-eslint/no-non-null-assertion': 'off',
            '@typescript-eslint/ban-ts-comment': 'off',
            'dot-notation': 'error',
            eqeqeq: ['error', 'smart'],
            'guard-for-in': 'error',

            'max-lines': [
                'error',
                {
                    max: 600,
                },
            ],

            'no-bitwise': 'error',
            'no-caller': 'error',

            'no-console': [
                'error',
                {
                    allow: ['info', 'warn', 'error'],
                },
            ],

            'no-else-return': 'error',
            'no-eval': 'error',
            'no-new-wrappers': 'error',
            'no-sequences': 'error',
            'no-throw-literal': 'error',
            'no-undef-init': 'error',
            'no-unused-expressions': 'error',
            'no-useless-concat': 'error',
            'no-useless-escape': 'error',
            'object-shorthand': 'error',
            'prefer-exponentiation-operator': 'error',
            'prefer-object-spread': 'error',
            'prefer-regex-literals': 'error',
            'prefer-template': 'error',
            radix: 'error',
            'spaced-comment': 'error',

            '@angular-eslint/directive-selector': [
                'error',
                {
                    type: 'attribute',
                    prefix: 'td',
                    style: 'camelCase',
                },
            ],

            '@angular-eslint/component-selector': [
                'error',
                {
                    type: 'element',
                    prefix: 'td',
                    style: 'kebab-case',
                },
            ],

            'no-empty-function': 'off',
            '@typescript-eslint/no-empty-function': 'off',
            'import/no-unresolved': 0,

            'import/order': [
                'error',
                {
                    groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'index', 'object', 'type', 'unknown'],

                    'newlines-between': 'never',

                    alphabetize: {
                        order: 'asc',
                        caseInsensitive: true,
                    },
                },
            ],
        },
    },

    {
        files: ['**/*.spec.ts'],
        rules: {
            '@typescript-eslint/ban-ts-comment': 'off',
            '@typescript-eslint/no-floating-promises': 'off',
            '@angular-eslint/use-component-selector': 'off',
            'max-lines-per-function': 'off',
            'max-lines': 'off',
        },
    },

    {
        files: ['**/*.decorator.ts'],
        rules: {
            'max-lines-per-function': 'off',
        },
    },

    {
        files: ['**/*.html'],
        languageOptions: {
            parser: angularTemplateParser,
        },
        plugins: {
            '@angular-eslint/template': angularTemplatePlugin,
        },
        rules: {
            ...angularTemplatePlugin.configs.recommended.rules,
            '@angular-eslint/template/prefer-self-closing-tags': 'error',
            '@angular-eslint/template/prefer-control-flow': 'error',
        },
    },

    {
        files: ['**/*.html'],
        ignores: ['**/*inline-template-*.component.html'],
        plugins: {
            prettier: prettierPlugin,
        },
        rules: {
            'prettier/prettier': [
                'error',
                {
                    parser: 'angular',
                },
            ],
        },
    },
]);
