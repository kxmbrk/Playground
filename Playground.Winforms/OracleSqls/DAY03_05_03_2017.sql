--05/03/2017: DAY 3: FUNCTIONS
--EX: 3-1
SET SERVEROUTPUT ON;
CREATE OR REPLACE FUNCTION GET_JOB(P_JOB_ID JOBS.JOB_ID%TYPE)
RETURN VARCHAR2
AS
       V_TITLE JOBS.JOB_TITLE%TYPE;
BEGIN
      SELECT JOB_TITLE INTO V_TITLE
      FROM JOBS 
      WHERE JOB_ID = P_JOB_ID;
      
      RETURN V_TITLE;
END GET_JOB;

--
SELECT TEXT FROM USER_SOURCE
WHERE TYPE = 'PROCEDURE'
AND NAME = 'PROC_POPULATE_DV_TABLES_MAT'
ORDER BY LINE;
---
/
VARIABLE B_TITLE VARCHAR2(100);
BEGIN
     :B_TITLE := GET_JOB('SA_REP');
END/;
/
PRINT B_TITLE;
--EX: 3-2
CREATE OR REPLACE FUNCTION GET_ANNUAL_COMP(
          P_SALARY EMPLOYEES.SALARY%TYPE,
          P_COMMISSION_PCT NUMBER)
RETURN NUMBER
AS
BEGIN
      IF(P_SALARY IS NOT NULL AND P_COMMISSION_PCT IS NOT NULL)THEN
         RETURN ((P_SALARY * 12) + (P_COMMISSION_PCT * P_SALARY * 12));
      ELSIF(P_SALARY IS NOT NULL AND P_COMMISSION_PCT IS NULL)THEN
          RETURN (P_SALARY * 12);
      ELSE 
          RETURN 0;
      END IF;
      
END GET_ANNUAL_COMP;
---
SELECT * FROM EMPLOYEES WHERE DEPARTMENT_ID = 30;
SELECT EMPLOYEE_ID, 
       LAST_NAME,
       GET_ANNUAL_COMP(SALARY, COMMISSION_PCT) AS ANNUAL_COMPENSATION
FROM EMPLOYEES
WHERE DEPARTMENT_ID = 30;
----EX: 3-3
CREATE OR REPLACE FUNCTION VALID_DEPTID(
          P_DEPTID DEPARTMENTS.DEPARTMENT_ID%TYPE)
RETURN BOOLEAN
AS 
       V_COUNTER NUMBER;
BEGIN
      SELECT COUNT(*) INTO V_COUNTER
      FROM DEPARTMENTS
      WHERE DEPARTMENT_ID = P_DEPTID;
      
      IF(V_COUNTER = 0)THEN
        RETURN FALSE;
      ELSE  
         RETURN TRUE;
      END IF;
END VALID_DEPTID;
select * from departments;
--
SELECT * FROM EMPLOYEES;
CREATE OR REPLACE PROCEDURE ADD_EMPLOYEE(
          P_FIRST_NAME EMPLOYEES.FIRST_NAME%TYPE,
          P_LAST_NAME EMPLOYEES.LAST_NAME%TYPE,
          P_EMAIL EMPLOYEES.EMAIL%TYPE,
          P_JOB EMPLOYEES.JOB_ID%TYPE DEFAULT 'SA_REP',
          P_MGR EMPLOYEES.MANAGER_ID%TYPE DEFAULT 145,
          P_SAL EMPLOYEES.SALARY%TYPE DEFAULT 1000,
          P_COMM EMPLOYEES.COMMISSION_PCT%TYPE DEFAULT 0,
          P_DEPTID EMPLOYEES.DEPARTMENT_ID%TYPE DEFAULT 30)

AS
BEGIN 
       IF(VALID_DEPTID(P_DEPTID))THEN
          INSERT INTO EMPLOYEES(EMPLOYEE_ID, FIRST_NAME, LAST_NAME, EMAIL, HIRE_DATE, JOB_ID, MANAGER_ID, SALARY, COMMISSION_PCT, DEPARTMENT_ID)
          VALUES(EMPLOYEES_SEQ.NEXTVAL, 
                 P_FIRST_NAME, 
                 P_LAST_NAME, 
                 P_EMAIL, 
                 TRUNC(SYSDATE), 
                 P_JOB, 
                 P_MGR, 
                 P_SAL, 
                 P_COMM, 
                 P_DEPTID);
                 
                 DBMS_OUTPUT.PUT_LINE('Employee: ' || P_FIRST_NAME || ' ' || P_LAST_NAME || ' added successfully.');
       END IF;
END ADD_EMPLOYEE;
--
CREATE OR REPLACE FUNCTION GET_LOCATION(P_DEPT_NAME VARCHAR2)
RETURN VARCHAR2
AS
      V_DEPT_NAME VARCHAR2(30);
BEGIN
      SELECT DEPARTMENT_NAME INTO V_DEPT_NAME
      FROM DEPARTMENTS 
      WHERE DEPARTMENT_NAME = P_DEPT_NAME;
      
      RETURN V_DEPT_NAME;
END;

---
create or replace PROCEDURE EMP_LIST
(P_MAXROWS NUMBER)
AS
     CURSOR CUR_EMP IS
     SELECT D.DEPARTMENT_NAME, E.EMPLOYEE_ID, E.LAST_NAME, 
     E.SALARY, E.COMMISSION_PCT
     FROM 
     DEPARTMENTS D, 
     EMPLOYEES E
     WHERE D.DEPARTMENT_ID = E.DEPARTMENT_ID;
     
     REC_EMP CUR_EMP%ROWTYPE;
     TYPE EMP_TAB_TYPE IS TABLE OF CUR_EMP%ROWTYPE INDEX BY BINARY_INTEGER;
     EMP_TAB EMP_TAB_TYPE;
     i NUMBER := 1;
     V_CITY VARCHAR2(30);
BEGIN
      OPEN CUR_EMP;
      FETCH CUR_EMP INTO REC_EMP;
      EMP_TAB(i) := REC_EMP;
      WHILE(CUR_EMP%FOUND AND i <= P_MAXROWS)
      LOOP
           i := i + 1;
           FETCH CUR_EMP INTO REC_EMP;
           EMP_TAB(i) := REC_EMP;
           V_CITY := GET_LOCATION(REC_EMP.DEPARTMENT_NAME);
           DBMS_OUTPUT.PUT_LINE('Employee ' || REC_EMP.LAST_NAME || ' works in ' || V_CITY);
      END LOOP;
      CLOSE CUR_EMP;
      
      FOR j IN REVERSE 1 .. i
      LOOP
          DBMS_OUTPUT.PUT_LINE(EMP_TAB(j).LAST_NAME);
      END LOOP;
      
END EMP_LIST;
--------------CHAPTER 4: PACKAGES
CREATE OR REPLACE PACKAGE JOB_PKG
AS
   PROCEDURE ADD_JOB(
             P_JOB_ID JOBS.JOB_ID%TYPE,
             P_JOB_TITLE JOBS.JOB_TITLE%TYPE);
             
   PROCEDURE DEL_JOB(P_JOB_ID JOBS.JOB_ID%TYPE);
   
   FUNCTION GET_JOB(P_JOB_ID JOBS.JOB_ID%TYPE)
            RETURN JOBS.JOB_TITLE%TYPE;
            
   PROCEDURE UPD_JOB(
             P_JOB_ID JOBS.JOB_ID%TYPE,
             P_JPB_TITLE JOBS.JOB_TITLE%TYPE);

END JOB_PKG;


CREATE OR REPLACE
PACKAGE BODY JOB_PKG AS

  PROCEDURE ADD_JOB(
             P_JOB_ID JOBS.JOB_ID%TYPE,
             P_JOB_TITLE JOBS.JOB_TITLE%TYPE) AS
  BEGIN
         INSERT INTO JOBS(JOB_ID, JOB_TITLE)
         VALUES(P_JOB_ID, P_JOB_TITLE);
         COMMIT;
  END ADD_JOB;

  PROCEDURE DEL_JOB(P_JOB_ID JOBS.JOB_ID%TYPE) AS
  BEGIN
        DELETE FROM JOBS
        WHERE JOB_ID = P_JOB_ID;
        
        IF(SQL%NOTFOUND) THEN
          RAISE_APPLICATION_ERROR(-20203, 'No jobs deleted.');
        END IF;
        
  END DEL_JOB;


  FUNCTION GET_JOB(P_JOB_ID JOBS.JOB_ID%TYPE)
            RETURN JOBS.JOB_TITLE%TYPE 
  AS
            V_JOB_TITLE JOBS.JOB_TITLE%TYPE;
  BEGIN
         SELECT JOB_TITLE INTO V_JOB_TITLE
         FROM JOBS
         WHERE JOB_ID = P_JOB_ID;
         
         RETURN V_JOB_TITLE;
    
  END GET_JOB;

  PROCEDURE UPD_JOB(
             P_JOB_ID JOBS.JOB_ID%TYPE,
             P_JPB_TITLE JOBS.JOB_TITLE%TYPE) AS
  BEGIN
          UPDATE JOBS 
          SET JOB_TITLE = P_JPB_TITLE
          WHERE JOB_ID = P_JOB_ID;
          
          IF(SQL%NOTFOUND)THEN
             RAISE_APPLICATION_ERROR(-20202, 'No Job updated.');
          END IF;
          
  END UPD_JOB;

END JOB_PKG;

----
select * from jobs;
CREATE OR REPLACE PACKAGE EMP_PKG
AS
   PROCEDURE ADD_EMPLOYEE(
          P_FIRST_NAME EMPLOYEES.FIRST_NAME%TYPE,
          P_LAST_NAME EMPLOYEES.LAST_NAME%TYPE,
          P_EMAIL EMPLOYEES.EMAIL%TYPE,
          P_JOB EMPLOYEES.JOB_ID%TYPE DEFAULT 'SA_REP',
          P_MGR EMPLOYEES.MANAGER_ID%TYPE DEFAULT 145,
          P_SAL EMPLOYEES.SALARY%TYPE DEFAULT 1000,
          P_COMM EMPLOYEES.COMMISSION_PCT%TYPE DEFAULT 0,
          P_DEPTID EMPLOYEES.DEPARTMENT_ID%TYPE DEFAULT 30);
          
   FUNCTION VALID_DEPTID(P_DEPTID DEPARTMENTS.DEPARTMENT_ID%TYPE)
         RETURN BOOLEAN;
         
   PROCEDURE GET_EMPLOYEE(
             P_EMP_ID EMPLOYEES.EMPLOYEE_ID%TYPE,
             P_SAL OUT EMPLOYEES.SALARY%TYPE,
             P_JOB OUT EMPLOYEES.JOB_ID%TYPE);
END EMP_PKG;

CREATE OR REPLACE
PACKAGE BODY EMP_PKG AS

  PROCEDURE ADD_EMPLOYEE(
          P_FIRST_NAME EMPLOYEES.FIRST_NAME%TYPE,
          P_LAST_NAME EMPLOYEES.LAST_NAME%TYPE,
          P_EMAIL EMPLOYEES.EMAIL%TYPE,
          P_JOB EMPLOYEES.JOB_ID%TYPE DEFAULT 'SA_REP',
          P_MGR EMPLOYEES.MANAGER_ID%TYPE DEFAULT 145,
          P_SAL EMPLOYEES.SALARY%TYPE DEFAULT 1000,
          P_COMM EMPLOYEES.COMMISSION_PCT%TYPE DEFAULT 0,
          P_DEPTID EMPLOYEES.DEPARTMENT_ID%TYPE DEFAULT 30) AS
  BEGIN
           IF(VALID_DEPTID(P_DEPTID))THEN
          INSERT INTO EMPLOYEES(EMPLOYEE_ID, FIRST_NAME, LAST_NAME, EMAIL, HIRE_DATE, JOB_ID, MANAGER_ID, SALARY, COMMISSION_PCT, DEPARTMENT_ID)
          VALUES(EMPLOYEES_SEQ.NEXTVAL, 
                 P_FIRST_NAME, 
                 P_LAST_NAME, 
                 P_EMAIL, 
                 TRUNC(SYSDATE), 
                 P_JOB, 
                 P_MGR, 
                 P_SAL, 
                 P_COMM, 
                 P_DEPTID);
                 
                 DBMS_OUTPUT.PUT_LINE('Employee: ' || P_FIRST_NAME || ' ' || P_LAST_NAME || ' added successfully.');
       END IF;
   exception 
      when others then 
       DBMS_OUTPUT.PUT_LINE(SQLERRM);
END ADD_EMPLOYEE;

  FUNCTION VALID_DEPTID(P_DEPTID DEPARTMENTS.DEPARTMENT_ID%TYPE)
         RETURN BOOLEAN 
  AS
         V_COUNTER NUMBER;
  BEGIN
          SELECT COUNT(*) INTO V_COUNTER
      FROM DEPARTMENTS
      WHERE DEPARTMENT_ID = P_DEPTID;
      
      IF(V_COUNTER = 0)THEN
        RETURN FALSE;
      ELSE  
         RETURN TRUE;
      END IF;
END VALID_DEPTID;

  PROCEDURE GET_EMPLOYEE(
             P_EMP_ID EMPLOYEES.EMPLOYEE_ID%TYPE,
             P_SAL OUT EMPLOYEES.SALARY%TYPE,
             P_JOB OUT EMPLOYEES.JOB_ID%TYPE) 
  AS
  BEGIN
        SELECT SALARY, JOB_ID 
        INTO P_SAL, P_JOB
        FROM EMPLOYEES 
        WHERE EMPLOYEE_ID = P_EMP_ID;
        
  END GET_EMPLOYEE;

END EMP_PKG;
-----chapter 5: working with packages
--ADD PROCEDURE IN THE EMP_PKG
PROCEDURE ADD_EMPLOYEE(
          P_FIRST_NAME EMPLOYEES.FIRST_NAME%TYPE,
          P_LAST_NAME EMPLOYEES.LAST_NAME%TYPE,
          P_DEPT_ID EMPLOYEES.DEPARTMENT_ID%TYPE);
----
PROCEDURE ADD_EMPLOYEE(
          P_FIRST_NAME EMPLOYEES.FIRST_NAME%TYPE,
          P_LAST_NAME EMPLOYEES.LAST_NAME%TYPE,
          P_DEPT_ID EMPLOYEES.DEPARTMENT_ID%TYPE)
AS
BEGIN
       
END ADD_EMPLOYEE;

---
SELECT * FROM EMPLOYEES WHERE FIRST_NAME = 'James';
DECLARE
BEGIN
       EMP_PKG.PRINT_EMPLOYEE(EMP_PKG.GET_EMPLOYEE(194));
       EMP_PKG.PRINT_EMPLOYEE(EMP_PKG.GET_EMPLOYEE('Joplin'));
END;
---
BEGIN
     EMP_PKG.INIT_DEPARTMENTS;
END;
--
SELECT * FROM DEPARTMENTS;
INSERT INTO DEPARTMENTS(DEPARTMENT_ID, DEPARTMENT_NAME)
VALUES(15, 'Security');
commit;
---
DELETE FROM EMPLOYEES WHERE FIRST_NAME = 'James' and last_name = 'Bond';
delete from departments where department_name = 'Security';
commit;
------chapter 6: Oracle supplied packages
create or replace PROCEDURE EMPLOYEE_REPORT(
        P_OUTPUT_DIR VARCHAR2,
        P_FILE_NAME VARCHAR2)
AS
      f UTL_FILE.FILE_TYPE;
      CURSOR CUR_AVG IS
      SELECT LAST_NAME,
             DEPARTMENT_ID,
             SALARY
    FROM EMPLOYEES OUTER
    WHERE SALARY >
      (SELECT AVG(SALARY)
      FROM EMPLOYEES INNER
      WHERE DEPARTMENT_ID = OUTER.DEPARTMENT_ID
      )
    ORDER BY DEPARTMENT_ID;
BEGIN
      f := UTL_FILE.FOPEN(P_OUTPUT_DIR, P_FILE_NAME, 'W');
      UTL_FILE.PUT_LINE(f, 'Employee who earn more than average salary: ');
      UTL_FILE.PUT_LINE(f, 'Report Generated on ' || SYSDATE);
      UTL_FILE.NEW_LINE(f);
      
      FOR EMP IN CUR_AVG
      LOOP
           UTL_FILE.PUT_LINE(f, 
           RPAD(EMP.LAST_NAME, 30) || ' ' ||
           LPAD(NVL(TO_CHAR(EMP.DEPARTMENT_ID, '9999'), '-'), 5) || ' ' ||
           LPAD(TO_CHAR(EMP.SALARY, '$99,999.00'), 12));
      END LOOP;
      
      UTL_FILE.NEW_LINE(f);
      UTL_FILE.PUT_LINE(f, '***  END OF REPORT  ***');
      UTL_FILE.FCLOSE(f);
      
      EXCEPTION
          WHEN OTHERS THEN
            SYS.DBMS_OUTPUT.PUT_LINE('Failed to generate report. ' || SQLERRM);
END EMPLOYEE_REPORT;

---
SELECT LAST_NAME,
  DEPARTMENT_ID,
  SALARY
FROM EMPLOYEES OUTER
WHERE SALARY >
  (SELECT AVG(SALARY)
  FROM EMPLOYEES INNER
  WHERE DEPARTMENT_ID = OUTER.DEPARTMENT_ID
  )
ORDER BY DEPARTMENT_ID;