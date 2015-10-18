/*
SQLyog Community v12.15 (64 bit)
MySQL - 5.5.44-0+deb8u1 : Database - AuthServer
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`AuthServer` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `AuthServer`;

/*Table structure for table `Bans` */

DROP TABLE IF EXISTS `Bans`;

CREATE TABLE `Bans` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` int(11) NOT NULL,
  `time` datetime NOT NULL,
  `reason` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `Realms` */

DROP TABLE IF EXISTS `Realms`;

CREATE TABLE `Realms` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `address` varchar(15) NOT NULL,
  `port` int(11) NOT NULL,
  `lastactive` datetime DEFAULT NULL,
  `visible` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

/*Table structure for table `Users` */

DROP TABLE IF EXISTS `Users`;

CREATE TABLE `Users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(30) NOT NULL,
  `mailaddress` varchar(254) NOT NULL,
  `password` varchar(64) NOT NULL,
  `salt` varchar(64) NOT NULL,
  `rank` tinyint(4) NOT NULL DEFAULT '0',
  `active` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

/* Function  structure for function  `AuthenticateUser` */

/*!50003 DROP FUNCTION IF EXISTS `AuthenticateUser` */;
DELIMITER $$

/*!50003 CREATE DEFINER=`silvester`@`%` FUNCTION `AuthenticateUser`(
	in_username 	VARCHAR(30), 
	in_password 	VARCHAR(30)
) RETURNS tinyint(4)
BEGIN
	/* This Function can return the following values:
		0 - Successfully authenticated the user.
		1 - Username/Password do not match.
	*/
	
	# Start off by declaring some basic variables.
	DECLARE userid		INT;
	DECLARE out_status	TINYINT;
		
	# Get our userid, if we have one.
	SELEct 		u.id 
	from 		Users 	u
	left JOIN 	Bans 	b
	on 		u.id = b.user
	where u.username = in_username and u.password = SHA2(concat(u.salt, SHA2(in_password, 256)), 256) 
	and u.active = 1 AND b.id IS NULL
	LIMIT 1 INTO userid;
	# Check if we have a matching user.
	if not isnull(userid) then
		set out_status = 0;
	else
		set out_status = 1;
	end if;
	
	return (out_status);
	
    END */$$
DELIMITER ;

/* Function  structure for function  `GetUserId` */

/*!50003 DROP FUNCTION IF EXISTS `GetUserId` */;
DELIMITER $$

/*!50003 CREATE DEFINER=`silvester`@`%` FUNCTION `GetUserId`(
	in_username varchar(30)
) RETURNS int(11)
BEGIN
	DECLARE out_id int;
	select `id` from Users where `username` = in_username into out_id;
	return out_id;
    END */$$
DELIMITER ;

/* Function  structure for function  `RegisterUser` */

/*!50003 DROP FUNCTION IF EXISTS `RegisterUser` */;
DELIMITER $$

/*!50003 CREATE DEFINER=`silvester`@`%` FUNCTION `RegisterUser`(
	in_username 	VARCHAR(30), 
	in_password 	VARCHAR(30), 
	in_mailaddress 	VARCHAR(254)
) RETURNS tinyint(4)
BEGIN
	/* This Function can return the following values:
		0 - Successfully created an account.
		1 - Username/Mail Address already in use.
		2 - Username/Password too short.
		3 - Invalid Mail Address.
	*/
    
	# Start off by declaring some basic variables.
	DECLARE salt 		VARCHAR(64);
	DECLARE hashedpwd 	VARCHAR(64);
	DECLARE userid		INT;
	DECLARE out_status	tinyint;
	
	# Make sure the Username and Password have the correct length.
	if char_length(in_username) > 3 or char_length(in_password) > 3 THEN
		# Check if we have a valid mail address here.
		if in_mailaddress LIKE '%@%.%' then
			# Check if the user already exists.
			SELECT `id` FROM Users WHERE `username` = in_username OR `mailaddress` = in_mailaddress LIMIT 1 INTO userid;
			IF ISNULL(userid) THEN
				# Register our new user.
				SET salt 		= SHA2(CONCAT(in_username, NOW()), 256);
				SET hashedpwd		= SHA2(CONCAT(salt, SHA2(in_password, 256)), 256);
				INSERT INTO Users(`username`, `mailaddress`, `password`, `salt`) VALUES (in_username, in_mailaddress, hashedpwd, salt);
				
				# Return 
				SET out_status = 0;
			ELSE
				# Return a false;
				SET out_status = 1;
			END IF;
		else
			set out_status = 3;
		end if;
	ELSE
		set out_status = 2;
	end if;
	
	RETURN (out_status);
    END */$$
DELIMITER ;

/* Procedure structure for procedure `ActivateUser` */

/*!50003 DROP PROCEDURE IF EXISTS  `ActivateUser` */;

DELIMITER $$

/*!50003 CREATE DEFINER=`silvester`@`%` PROCEDURE `ActivateUser`(
	in_id INT
)
BEGIN
	
	update Users set `active` = 1 where `id` = in_id;
	
    END */$$
DELIMITER ;

/* Procedure structure for procedure `AddRealm` */

/*!50003 DROP PROCEDURE IF EXISTS  `AddRealm` */;

DELIMITER $$

/*!50003 CREATE DEFINER=`silvester`@`%` PROCEDURE `AddRealm`(
	in_name 	varchar(50),
	in_address 	varchar(15),
	in_port		Int,
	in_visible	tinyint
)
BEGIN
	INSERT INTO `Realms` (`name`, `address`, `port`, `visible`) 
	VALUES (in_name, in_address, in_port, in_visible);
    END */$$
DELIMITER ;

/*Table structure for table `RealmList` */

DROP TABLE IF EXISTS `RealmList`;

/*!50001 DROP VIEW IF EXISTS `RealmList` */;
/*!50001 DROP TABLE IF EXISTS `RealmList` */;

/*!50001 CREATE TABLE  `RealmList`(
 `name` varchar(50) ,
 `address` varchar(15) ,
 `port` int(11) ,
 `lastactive` datetime 
)*/;

/*View structure for view RealmList */

/*!50001 DROP TABLE IF EXISTS `RealmList` */;
/*!50001 DROP VIEW IF EXISTS `RealmList` */;

/*!50001 CREATE ALGORITHM=UNDEFINED DEFINER=`silvester`@`%` SQL SECURITY DEFINER VIEW `RealmList` AS (select `Realms`.`name` AS `name`,`Realms`.`address` AS `address`,`Realms`.`port` AS `port`,`Realms`.`lastactive` AS `lastactive` from `Realms` where (`Realms`.`visible` = 1)) */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
