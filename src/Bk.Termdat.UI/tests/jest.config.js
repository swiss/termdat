'use strict';

/** @type {import('jest-preset-angular').Config} */
module.exports = {
    maxWorkers: 2,
    preset: 'jest-preset-angular',
    setupFilesAfterEnv: ['<rootDir>/tests/setupJest.ts'],
    moduleNameMapper: {
        '^api/(.*)': '<rootDir>/src/api/$1',
    },
    workerIdleMemoryLimit: '512MB',
};
