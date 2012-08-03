BEGIN TRANSACTION;
      SET CONSTRAINTS ALL DEFERRED;

	  INSERT INTO inst(inst_id, inst_contact_persona_id, inst_name, inst_status)
	  		 VALUES (DEFAULT, 1, 'test', 'ACTIVE');

	  INSERT INTO persona(persona_id, inst_id, type, prefix, name, paterno, materno, gender, picture, remarks, status)
	  		 VALUES (DEFAULT, 1, 'INST', NULL, 'Test', NULL, NULL, 'MALE', NULL, NULL, 'ACTIVE');

      -- password is ``blah''
      INSERT INTO account(persona_id, inst_id, username, passwd, status)
      	     VALUES (1, 1, 'testuser', '6f1ed002ab5595859014ebf0951522d9', 'ACTIVE');

COMMIT TRANSACTION;
