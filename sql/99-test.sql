BEGIN TRANSACTION;
      SET CONSTRAINTS ALL DEFERRED;

	  INSERT INTO inst(inst_id, inst_contact_persona_id, inst_name, inst_status)
	  		 VALUES (DEFAULT, 1, 'test', 'ACTIVE');

	  INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, picture, remarks, status)
	  		 VALUES (DEFAULT, 1, 'INST', NULL, 'Test', NULL, NULL, 'FEMALE', NULL, NULL, 'ACTIVE');

      -- password is ``blah''
      INSERT INTO account(persona_id, inst_id, username, passwd, account_type, status)
      	     VALUES (1, 1, 'testuser', '6f1ed002ab5595859014ebf0951522d9', 'ADMIN', 'ACTIVE');

	  INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, picture, remarks, status)
	  		 VALUES (DEFAULT, 1, 'USER', 'Mr.', 'Alfonso', 'Otero', NULL, 'MALE', NULL, NULL, 'ACTIVE');

      -- password is ``blah''
      INSERT INTO account(persona_id, inst_id, username, passwd, account_type, status)
      	     VALUES (2, 1, 'tito', 'd0655af3824a90cf215bedc890a9028a', 'SUPERUSER', 'ACTIVE');

COMMIT TRANSACTION;
