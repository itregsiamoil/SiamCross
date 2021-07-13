INSERT INTO FieldDictionary(Id, Title)VALUES(0, 'UNKNOWN');

/* БД по принципу EAV (Entity attibute value)
однако все сущности (типы сущностей ) определены "жестко" в таблице Entitys
сущность не может содержать вложенных сущностей - только значения
0 - измерение, данные 
1 - измерения, доп.данные 
10 - устройство, данные протокола
11 - устройство, данные соединения
*/ 
INSERT INTO Entitys(KindId, Title)VALUES(0, 'измерение, данные ');
INSERT INTO Entitys(KindId, Title)VALUES(1, 'измерения, доп.данные');
INSERT INTO Entitys(KindId, Title)VALUES(10, 'устройство');
INSERT INTO Entitys(KindId, Title)VALUES(11, 'устройство, данные соединения');
INSERT INTO Entitys(KindId, Title)VALUES(12, 'устройство, данные протокола');
INSERT INTO Entitys(KindId, Title)VALUES(13, 'устройство, инфо о местоположении');
INSERT INTO Entitys(KindId, Title)VALUES(20, 'настройки почтового клиента');
/* в таблице Attributes определены имена и типы значений */

INSERT INTO Attributes(Title, TypeId)VALUES('sudcorrectiontype', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('sudcorrectiontypeud', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('sudresearchtype', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('sudpressure', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('lglevel', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('lglevelud', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('lgsoundspeed', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('lgreflectioncount', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('lgtimediscrete', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('lgechogram', 3);


INSERT INTO Attributes(Title, TypeId)VALUES('PeriodSec', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('MeasurementsCount', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('MinPressure', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('MaxPressure', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('MinIntTemperature', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('MaxIntTemperature', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('MinExtTemperature', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('MaxExtTemperature', 5);
--bd siam
INSERT INTO Attributes(Title, TypeId)VALUES('umttype', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('mtscalefactor', 1);
INSERT INTO Attributes(Title, TypeId)VALUES('mtinterval', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('mttemperature', 3);
INSERT INTO Attributes(Title, TypeId)VALUES('mtpressure', 3);
INSERT INTO Attributes(Title, TypeId)VALUES('umttemperatureex', 3);
-- additional info
INSERT INTO Attributes(Title, TypeId)VALUES('bufferpressure', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('pumpdepth', 5);
INSERT INTO Attributes(Title, TypeId)VALUES('holeindex', 1);


INSERT INTO Attributes(Title, TypeId)VALUES('Name', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('Mac', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('Guid', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('BondState', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('PrimaryPhy', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('SecondaryPhy', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('IsLegacy', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('IsConnectable', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('TxPower', 2);
INSERT INTO Attributes(Title, TypeId)VALUES('Rssi', 2);



