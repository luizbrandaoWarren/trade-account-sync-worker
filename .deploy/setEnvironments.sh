#!/usr/bin/env bash

echo "Env: $1"
case $1 in
master | main)
    echo "PKG_VERSION_SUFFIX=" >> $GITHUB_ENV
    echo "ECS_CLUSTER=production" >> $GITHUB_ENV
    echo "AWS_ACCESS_KEY_ID=PRD_AWS_ACCESS_KEY_ID" >> $GITHUB_ENV
    echo "AWS_SECRET_ACCESS_KEY=PRD_AWS_SECRET_ACCESS_KEY" >> $GITHUB_ENV
    echo "AWS_MIGRATION_PARAMETER_NAME=/prd_core/db-trade-account/connection-string/migrations" >> $GITHUB_ENV
    ;;

staging)
    echo "PKG_VERSION_SUFFIX=--version-suffix beta" >> $GITHUB_ENV
    echo "ECS_CLUSTER=staging" >> $GITHUB_ENV
    echo "AWS_ACCESS_KEY_ID=STG_AWS_ACCESS_KEY_ID" >> $GITHUB_ENV
    echo "AWS_SECRET_ACCESS_KEY=STG_AWS_SECRET_ACCESS_KEY" >> $GITHUB_ENV
    echo "AWS_MIGRATION_PARAMETER_NAME=/stg_core/db-trade-account/connection-string/migrations" >> $GITHUB_ENV
    ;;

*)
    echo "PKG_VERSION_SUFFIX=--version-suffix beta" >> $GITHUB_ENV
    echo "ECS_CLUSTER=staging" >> $GITHUB_ENV
    echo "AWS_ACCESS_KEY_ID=STG_AWS_ACCESS_KEY_ID" >> $GITHUB_ENV
    echo "AWS_SECRET_ACCESS_KEY=STG_AWS_SECRET_ACCESS_KEY" >> $GITHUB_ENV
    echo "AWS_MIGRATION_PARAMETER_NAME=/stg_core/db-trade-account/connection-string/migrations" >> $GITHUB_ENV
    ;;
esac