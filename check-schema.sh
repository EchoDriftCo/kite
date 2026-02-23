#!/bin/bash
psql "$DATABASE_URL" -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'CircleMember' AND table_schema = 'public';"
