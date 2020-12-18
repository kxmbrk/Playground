﻿CREATE TABLE ACCT_CATEGORY
(
  CATEGORY_ID NUMBER PRIMARY KEY NOT NULL, 
  CATEGORY_NAME VARCHAR2(30) NOT NULL
);

INSERT INTO ACCT_CATEGORY VALUES(1, 'DATABASE');
INSERT INTO ACCT_CATEGORY VALUES(2, 'DOT.NET');
INSERT INTO ACCT_CATEGORY VALUES(3, 'EMAILS');
INSERT INTO ACCT_CATEGORY VALUES(4, 'FINANCES');
INSERT INTO ACCT_CATEGORY VALUES(5, 'MISC');
INSERT INTO ACCT_CATEGORY VALUES(6, 'PERSONAL');
INSERT INTO ACCT_CATEGORY VALUES(7, 'SOCIAL NETWORK');
INSERT INTO ACCT_CATEGORY VALUES(8, 'WORK_RELATED');
COMMIT;

SELECT * FROM ACCT_CATEGORY;
--------------------------------------------------------------------------------
CREATE TABLE ACCT_MGR
(
  ID INT PRIMARY KEY NOT NULL, 
  ACCT_NAME VARCHAR2(30) NOT NULL, 
  ACCT_LOGIN_ID VARCHAR2(50) NOT NULL, 
  ACCT_PASSWORD VARCHAR2(100) NOT NULL, 
  ACCT_NOTES VARCHAR2(4000), 
  DATE_CREATED DATE, 
  DATE_MODIFIED DATE, 
  CATEGORY_ID INT,
  
  CONSTRAINT FK_CATEGORY 
  FOREIGN KEY (CATEGORY_ID)
  REFERENCES ACCT_CATEGORY(CATEGORY_ID)
)
--------------------------------------------------------------------------------
CREATE SEQUENCE ACCT_SEQ
START WITH 101
INCREMENT BY 1;

--------------------------------------------------------------------------------
SET DEFINE OFF;
CREATE OR REPLACE TRIGGER ACCT_PK_TRGR
 BEFORE INSERT
 ON ACCT_MGR
 FOR EACH ROW
 
BEGIN
      SELECT ACCT_SEQ.NEXTVAL 
      INTO :NEW.ID FROM DUAL; 
END;

---------------------------------------------------------------------------------------
CREATE TABLE ACCT_HISTORY
(
  TRANSACTION_TYPE VARCHAR2(15),
  TIME_STAMP TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  ID NUMBER,
  ACCT_NAME VARCHAR2(30),
  ACCT_LOGIN_ID VARCHAR2(50), 
  ACCT_PASSWORD VARCHAR2(100), 
  ACCT_NOTES VARCHAR2(4000),
  DATE_CREATED DATE, 
  DATE_MODIFIED DATE, 
  CATEGORY_ID NUMBER
);
-------------------------------------------------------------------------------------------
SET DEFINE OFF;
CREATE OR REPLACE TRIGGER ACCT_HIST_TRGR
    BEFORE UPDATE OR DELETE
    ON ACCT_MGR
    FOR EACH ROW
  BEGIN
       IF DELETING THEN
           INSERT INTO ACCT_HISTORY
                 (TRANSACTION_TYPE, ID, ACCT_NAME, ACCT_LOGIN_ID, ACCT_PASSWORD, 
                  ACCT_NOTES, DATE_CREATED, DATE_MODIFIED, CATEGORY_ID)
                  VALUES('DELETE', :OLD.ID, 
                                   :OLD.ACCT_NAME, 
                                   :OLD.ACCT_LOGIN_ID, 
                                   :OLD.ACCT_PASSWORD,
                                   :OLD.ACCT_NOTES, 
                                   :OLD.DATE_CREATED,
                                   :OLD.DATE_MODIFIED, 
                                   :OLD.CATEGORY_ID);
       ELSIF UPDATING THEN
             INSERT INTO ACCT_HISTORY
             (TRANSACTION_TYPE, ID, ACCT_NAME, ACCT_LOGIN_ID, ACCT_PASSWORD, 
              ACCT_NOTES, DATE_CREATED, DATE_MODIFIED, CATEGORY_ID)
             VALUES('UPDATE', :OLD.ID, 
                              :OLD.ACCT_NAME, 
                              :OLD.ACCT_LOGIN_ID, 
                              :OLD.ACCT_PASSWORD,
                              :OLD.ACCT_NOTES, 
                              :OLD.DATE_CREATED,
                              :OLD.DATE_MODIFIED, 
                              :OLD.CATEGORY_ID);
       END IF;
END;
-------------------------------------------------------------------------------------------------

CREATE OR REPLACE PACKAGE PKG_ACCT_MGR AS 

--CATEGORY
PROCEDURE PROC_INSERT_CATEGORY(P_CATEGORY_NAME VARCHAR2, P_CATEGORY_ID OUT NUMBER);
PROCEDURE PROC_DELETE_CATEGORY(P_CATEGORY_ID NUMBER);
PROCEDURE PROC_UPDATE_CATEGORY(P_CATEGORY_ID NUMBER, P_CATEGORY_NAME VARCHAR2);

--ACCOUNT
PROCEDURE PROC_DELETE_ACCOUNT(P_ACCOUNT_ID NUMBER);

PROCEDURE PROC_INSERT_ACCT(
          P_ACCT_NAME VARCHAR2, 
          P_ACCT_LOGIN VARCHAR2,
          P_ACCT_PASS VARCHAR2,
          P_ACCT_NOTES VARCHAR2,
          P_CATEGORY_ID NUMBER,
          OUT_ID OUT NUMBER);

END PKG_ACCT_MGR;
/


CREATE OR REPLACE PACKAGE BODY PKG_ACCT_MGR AS

PROCEDURE PROC_INSERT_CATEGORY(P_CATEGORY_NAME VARCHAR2, P_CATEGORY_ID OUT NUMBER) 
AS
  BEGIN
        IF(P_CATEGORY_NAME IS NOT NULL) THEN

            INSERT INTO ACCT_CATEGORY(CATEGORY_ID, CATEGORY_NAME)
            VALUES (ACCT_SEQ.NEXTVAL, P_CATEGORY_NAME);

            P_CATEGORY_ID := ACCT_SEQ.CURRVAL;

        END IF;
        
  END PROC_INSERT_CATEGORY;

PROCEDURE PROC_DELETE_CATEGORY(P_CATEGORY_ID NUMBER)
AS
    V_ERR_MSG VARCHAR2(4000) := NULL;
BEGIN
    BEGIN
      DELETE FROM ACCT_CATEGORY WHERE CATEGORY_ID = P_CATEGORY_ID;
    EXCEPTION
        WHEN OTHERS THEN
            V_ERR_MSG := 'An Error occured while deleting a category - ' || SQLCODE ||' -ERROR- '|| SQLERRM;
            RAISE_APPLICATION_ERROR(-20000, V_ERR_MSG);
    END;
      
END PROC_DELETE_CATEGORY;


PROCEDURE PROC_UPDATE_CATEGORY(P_CATEGORY_ID NUMBER, P_CATEGORY_NAME VARCHAR2)
AS
BEGIN
      UPDATE ACCT_CATEGORY SET CATEGORY_NAME = P_CATEGORY_NAME 
      WHERE CATEGORY_ID = P_CATEGORY_ID;
      
END PROC_UPDATE_CATEGORY;


PROCEDURE PROC_DELETE_ACCOUNT(P_ACCOUNT_ID NUMBER)
AS
BEGIN

      DELETE FROM ACCT_MGR WHERE ID = P_ACCOUNT_ID;
      
END PROC_DELETE_ACCOUNT;

    PROCEDURE PROC_INSERT_ACCT(
              P_ACCT_NAME VARCHAR2, 
              P_ACCT_LOGIN VARCHAR2,
              P_ACCT_PASS VARCHAR2,
              P_ACCT_NOTES VARCHAR2,
              P_CATEGORY_ID NUMBER,
              OUT_ID OUT NUMBER)
    AS
        V_ERR_MSG VARCHAR2(4000) := NULL;
    BEGIN
        BEGIN
            SELECT ACCT_SEQ.NEXTVAL INTO OUT_ID FROM DUAL;
            INSERT INTO ACCT_MGR(ID, ACCT_NAME, ACCT_LOGIN_ID, ACCT_PASSWORD, ACCT_NOTES, DATE_CREATED, CATEGORY_ID)
            VALUES(OUT_ID, P_ACCT_NAME, P_ACCT_LOGIN, P_ACCT_PASS, P_ACCT_NOTES, SYSTIMESTAMP, P_CATEGORY_ID);
        EXCEPTION
            WHEN OTHERS THEN
                OUT_ID := -999;
                V_ERR_MSG := 'An Error occured while inserting into ACCT_MGR table - ' || SQLCODE ||' -ERROR- '|| SQLERRM;
                RAISE_APPLICATION_ERROR(-20000, V_ERR_MSG);
        END;
    END PROC_INSERT_ACCT;

END PKG_ACCT_MGR;
/
