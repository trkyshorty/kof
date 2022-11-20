ALTER TABLE "main"."account" RENAME TO "_account_old_20220524";
CREATE TABLE "main"."account" ("id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,"accountid" varchar,"password" varchar,"path" varchar,"platform" varchar,"serverid" varchar DEFAULT 1,"characterid" varchar DEFAULT 1,"charactername" varchar DEFAULT None);
INSERT INTO "main"."sqlite_sequence" (name, seq) VALUES ('account', '42');
INSERT INTO "main"."account" ("id", "accountid", "password", "path", "platform") SELECT "id", "name", "hash", "path", "platform" FROM "main"."_account_old_20220524";
UPDATE "main"."account" SET "charactername" = "accountid";
INSERT INTO "main"."zone" ("id", "name", "scalex", "scaley", "image") VALUES (22, 'Moradon 2', 1025, 1025, 'moradon.png');
INSERT INTO "main"."zone" ("id", "name", "scalex", "scaley", "image") VALUES (5, 'Luferson Castle 2', 2050, 2050, 'karus2004.png');
UPDATE "main"."zone" SET "name" = 'Luferson Castle 1' WHERE rowid = 1;
UPDATE "main"."zone" SET "name" = 'Moradon 1' WHERE rowid = 21;
