#!/bin/bash
service nginx start
service postgresql start
dotnet lib/ChainSmart.dll
