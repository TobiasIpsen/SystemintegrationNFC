-- RASP PI
select * from students;
select * from events;
select * from EventRegistrations;


-- select s.name studentname
-- from students s
-- where cardid = '0000000000000000000000000000000000000000'
-- join events where e.id = s.cardid;


INSERT INTO EventRegistrations (StudentId, EventId)
-- VALUES ('0', '0');
 VALUES ('5', '0');


delete from eventregistrations
where studentid  = '5' and eventid = '0'



delete from students where id = '5';

