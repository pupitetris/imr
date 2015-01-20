BEGIN TRANSACTION;
      SET CONSTRAINTS ALL DEFERRED;

      SELECT setval('inst_inst_id_seq', 1, false);
      SELECT setval('persona_persona_id_seq', 1, false);

      INSERT INTO inst(inst_id, inst_contact_persona_id, country_id, inst_name, inst_status)
      	     VALUES (DEFAULT, 1, 146, 'test', 'ACTIVE');

      INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, remarks, p_status)
      	     VALUES (DEFAULT, 1, 'INST', NULL, 'Test', NULL, NULL, 'FEMALE', NULL, 'ACTIVE');

      -- password is ``blah''
      INSERT INTO account(persona_id, inst_id, username, passwd, account_type, status)
      	     VALUES (1, 1, 'testuser', '6f1ed002ab5595859014ebf0951522d9', 'ADMIN', 'ACTIVE');

      INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, remarks, p_status)
      	     VALUES (DEFAULT, 1, 'USER', 'Mr.', 'Alfonso', 'Otero', NULL, 'MALE', NULL, 'ACTIVE');

      -- password is ``nena''
      INSERT INTO account(persona_id, inst_id, username, passwd, account_type, status)
      	     VALUES (2, 1, 'tito', 'd0655af3824a90cf215bedc890a9028a', 'SUPERUSER', 'ACTIVE');

      INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, remarks, p_status)
      	     VALUES (DEFAULT, 1, 'USER', 'Dr.', 'Fulano', 'Sultano', NULL, 'MALE', NULL, 'ACTIVE');

      -- A pictures for tito.
      SELECT imr_persona_add_photo(1, 2);

      INSERT INTO account(persona_id, inst_id, username, passwd, account_type, status)
      	     VALUES (3, 1, 'fulano', 'd0655af3824a90cf215bedc890a9028a', 'OPERATOR', 'ACTIVE');

\echo
\echo Success.

COMMIT TRANSACTION;
