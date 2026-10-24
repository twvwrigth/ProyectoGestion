CREATE DATABASE  IF NOT EXISTS `dboptica` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `dboptica`;
-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: dboptica
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `accesorios`
--

DROP TABLE IF EXISTS `accesorios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accesorios` (
  `idAccesorios` int NOT NULL AUTO_INCREMENT,
  `idProducto` int NOT NULL,
  `nombre` varchar(30) NOT NULL,
  `existenciaMinima` int DEFAULT '5',
  PRIMARY KEY (`idAccesorios`),
  KEY `fk_accesorio_producto` (`idProducto`),
  CONSTRAINT `fk_accesorio_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `accesorios`
--

LOCK TABLES `accesorios` WRITE;
/*!40000 ALTER TABLE `accesorios` DISABLE KEYS */;
INSERT INTO `accesorios` VALUES (1,1,'siiiiiiiiiiiii',5);
/*!40000 ALTER TABLE `accesorios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `armazon`
--

DROP TABLE IF EXISTS `armazon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `armazon` (
  `codigoArmazon` int NOT NULL,
  `idProducto` int NOT NULL,
  `idClasePrecio` int DEFAULT NULL,
  `marca` varchar(20) DEFAULT NULL,
  `modelo` varchar(15) DEFAULT NULL,
  `material` varchar(100) DEFAULT NULL,
  `clase` varchar(15) DEFAULT NULL,
  `precio` decimal(10,2) DEFAULT NULL,
  `ventaSinGraduacion` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`codigoArmazon`),
  KEY `fk_armazon_producto` (`idProducto`),
  KEY `fk_armazon_clase` (`idClasePrecio`),
  CONSTRAINT `fk_armazon_clase` FOREIGN KEY (`idClasePrecio`) REFERENCES `clase_precio` (`idClasePrecio`),
  CONSTRAINT `fk_armazon_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `armazon`
--

LOCK TABLES `armazon` WRITE;
/*!40000 ALTER TABLE `armazon` DISABLE KEYS */;
INSERT INTO `armazon` VALUES (0,2,NULL,'123','456','CRY',NULL,NULL,1);
/*!40000 ALTER TABLE `armazon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bitacora`
--

DROP TABLE IF EXISTS `bitacora`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bitacora` (
  `idReporte` int NOT NULL AUTO_INCREMENT,
  `idEmpleado` int NOT NULL,
  `idEncargado` int NOT NULL,
  `FechaReporte` date NOT NULL,
  `motivo` varchar(255) DEFAULT NULL,
  `cambioRealizado` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`idReporte`),
  KEY `fk_bitacora_empleado` (`idEmpleado`),
  KEY `fk_bitacora_encargado` (`idEncargado`),
  CONSTRAINT `fk_bitacora_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_bitacora_encargado` FOREIGN KEY (`idEncargado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bitacora`
--

LOCK TABLES `bitacora` WRITE;
/*!40000 ALTER TABLE `bitacora` DISABLE KEYS */;
/*!40000 ALTER TABLE `bitacora` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cajaturno`
--

DROP TABLE IF EXISTS `cajaturno`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cajaturno` (
  `idCajaTurno` int NOT NULL AUTO_INCREMENT,
  `idEmpleado` int NOT NULL,
  `fechaApertura` datetime NOT NULL,
  `fondoInicial` decimal(10,2) NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'Abierta',
  `fechaCierre` datetime DEFAULT NULL,
  `efectivoEsperado` decimal(10,2) DEFAULT NULL,
  `efectivoContado` decimal(10,2) DEFAULT NULL,
  `diferencia` decimal(10,2) DEFAULT NULL,
  `comentarioDiferencia` text,
  PRIMARY KEY (`idCajaTurno`),
  KEY `fk_caja_empleado` (`idEmpleado`),
  CONSTRAINT `fk_caja_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cajaturno`
--

LOCK TABLES `cajaturno` WRITE;
/*!40000 ALTER TABLE `cajaturno` DISABLE KEYS */;
INSERT INTO `cajaturno` VALUES (1,2,'2026-10-24 10:58:34',0.00,'Cerrada','2026-10-24 10:59:43',0.00,0.00,0.00,NULL),(2,2,'2026-10-24 11:15:05',0.00,'Cerrada','2026-10-24 11:16:51',1.00,1.00,0.00,NULL);
/*!40000 ALTER TABLE `cajaturno` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clase_precio`
--

DROP TABLE IF EXISTS `clase_precio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clase_precio` (
  `idClasePrecio` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(20) NOT NULL,
  `rangoMinimo` int DEFAULT NULL,
  `rangoMaximo` int DEFAULT NULL,
  PRIMARY KEY (`idClasePrecio`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clase_precio`
--

LOCK TABLES `clase_precio` WRITE;
/*!40000 ALTER TABLE `clase_precio` DISABLE KEYS */;
/*!40000 ALTER TABLE `clase_precio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cliente`
--

DROP TABLE IF EXISTS `cliente`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cliente` (
  `idCliente` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(255) NOT NULL,
  `telefono` varchar(10) DEFAULT NULL,
  `avisoPrivacidad` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`idCliente`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cliente`
--

LOCK TABLES `cliente` WRITE;
/*!40000 ALTER TABLE `cliente` DISABLE KEYS */;
/*!40000 ALTER TABLE `cliente` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cotizacion`
--

DROP TABLE IF EXISTS `cotizacion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cotizacion` (
  `idCotizacion` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `idReceta` int NOT NULL,
  `fechaCotizo` date NOT NULL,
  `totalCalculado` decimal(10,2) NOT NULL,
  `fechaExpiracion` date NOT NULL,
  `bonificacionExamen` tinyint(1) DEFAULT '0',
  `receta` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`idCotizacion`),
  KEY `fk_cotizacion_cliente` (`idCliente`),
  KEY `fk_cotizacion_empleado` (`idEmpleado`),
  KEY `fk_cotizacion_receta` (`idReceta`),
  CONSTRAINT `fk_cotizacion_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_cotizacion_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_cotizacion_receta` FOREIGN KEY (`idReceta`) REFERENCES `receta` (`idReceta`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cotizacion`
--

LOCK TABLES `cotizacion` WRITE;
/*!40000 ALTER TABLE `cotizacion` DISABLE KEYS */;
/*!40000 ALTER TABLE `cotizacion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empleado`
--

DROP TABLE IF EXISTS `empleado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empleado` (
  `idEmpleado` int NOT NULL AUTO_INCREMENT,
  `idRol` int NOT NULL,
  `idLaboratorio` int DEFAULT NULL,
  `nombre` varchar(255) NOT NULL,
  `usuario` varchar(40) NOT NULL,
  `contrasena` varchar(40) NOT NULL,
  `activo` tinyint(1) DEFAULT '1',
  `intentosContrasena` int DEFAULT '0',
  PRIMARY KEY (`idEmpleado`),
  UNIQUE KEY `usuario` (`usuario`),
  KEY `fk_empleado_rol` (`idRol`),
  KEY `fk_empleado_laboratorio` (`idLaboratorio`),
  CONSTRAINT `fk_empleado_laboratorio` FOREIGN KEY (`idLaboratorio`) REFERENCES `laboratorio` (`idLaboratorio`),
  CONSTRAINT `fk_empleado_rol` FOREIGN KEY (`idRol`) REFERENCES `rol` (`idRol`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleado`
--

LOCK TABLES `empleado` WRITE;
/*!40000 ALTER TABLE `empleado` DISABLE KEYS */;
INSERT INTO `empleado` VALUES (1,4,1,'Emmanuel','kngmtzz','123456789',1,0),(2,1,1,'Edgar','edgar','1',1,0);
/*!40000 ALTER TABLE `empleado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gafas_sol`
--

DROP TABLE IF EXISTS `gafas_sol`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gafas_sol` (
  `idGafasSol` int NOT NULL AUTO_INCREMENT,
  `idProducto` int NOT NULL,
  `nombre` varchar(30) NOT NULL,
  `existenciaMinima` int DEFAULT '5',
  PRIMARY KEY (`idGafasSol`),
  KEY `fk_gafas_producto` (`idProducto`),
  CONSTRAINT `fk_gafas_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gafas_sol`
--

LOCK TABLES `gafas_sol` WRITE;
/*!40000 ALTER TABLE `gafas_sol` DISABLE KEYS */;
INSERT INTO `gafas_sol` VALUES (1,3,'NOSE',5);
/*!40000 ALTER TABLE `gafas_sol` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inventario`
--

DROP TABLE IF EXISTS `inventario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventario` (
  `idEntrada` int NOT NULL AUTO_INCREMENT,
  `idProducto` int NOT NULL,
  `proveedor` varchar(30) DEFAULT NULL,
  `fechaEntrada` date NOT NULL,
  `cantidad` int NOT NULL,
  PRIMARY KEY (`idEntrada`),
  KEY `fk_inventario_producto` (`idProducto`),
  CONSTRAINT `fk_inventario_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventario`
--

LOCK TABLES `inventario` WRITE;
/*!40000 ALTER TABLE `inventario` DISABLE KEYS */;
/*!40000 ALTER TABLE `inventario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `laboratorio`
--

DROP TABLE IF EXISTS `laboratorio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `laboratorio` (
  `idLaboratorio` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `telefono` varchar(10) DEFAULT NULL,
  `tiempoEntrega` varchar(50) DEFAULT NULL,
  `tipoLaboratorio` varchar(7) DEFAULT NULL,
  `activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`idLaboratorio`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `laboratorio`
--

LOCK TABLES `laboratorio` WRITE;
/*!40000 ALTER TABLE `laboratorio` DISABLE KEYS */;
INSERT INTO `laboratorio` VALUES (1,'Ejemplo labo','2281000001','3 a 6 dias habiles','Externo',1);
/*!40000 ALTER TABLE `laboratorio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `lista_espera`
--

DROP TABLE IF EXISTS `lista_espera`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lista_espera` (
  `idLista` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `horaLlegada` datetime NOT NULL,
  `ClienteAtendido` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`idLista`),
  KEY `fk_lista_cliente` (`idCliente`),
  CONSTRAINT `fk_lista_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lista_espera`
--

LOCK TABLES `lista_espera` WRITE;
/*!40000 ALTER TABLE `lista_espera` DISABLE KEYS */;
/*!40000 ALTER TABLE `lista_espera` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mica`
--

DROP TABLE IF EXISTS `mica`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mica` (
  `idMica` int NOT NULL AUTO_INCREMENT,
  `material` varchar(50) NOT NULL,
  `tratamiento` varchar(50) NOT NULL,
  `rangoGraduacion` varchar(50) NOT NULL,
  `precio` decimal(10,2) NOT NULL,
  PRIMARY KEY (`idMica`),
  UNIQUE KEY `uk_mica_combinacion` (`material`,`tratamiento`,`rangoGraduacion`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mica`
--

LOCK TABLES `mica` WRITE;
/*!40000 ALTER TABLE `mica` DISABLE KEYS */;
INSERT INTO `mica` VALUES (7,'CR-39','Ninguno','2',1.00),(8,'CR-39','Ninguno','3',1.00);
/*!40000 ALTER TABLE `mica` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `movimientocaja`
--

DROP TABLE IF EXISTS `movimientocaja`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `movimientocaja` (
  `idMovimiento` int NOT NULL AUTO_INCREMENT,
  `idCajaTurno` int NOT NULL,
  `tipo` varchar(20) NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `motivo` varchar(255) NOT NULL,
  `fecha` datetime NOT NULL,
  PRIMARY KEY (`idMovimiento`),
  KEY `fk_movimiento_caja` (`idCajaTurno`),
  CONSTRAINT `fk_movimiento_caja` FOREIGN KEY (`idCajaTurno`) REFERENCES `cajaturno` (`idCajaTurno`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `movimientocaja`
--

LOCK TABLES `movimientocaja` WRITE;
/*!40000 ALTER TABLE `movimientocaja` DISABLE KEYS */;
INSERT INTO `movimientocaja` VALUES (1,2,'Entrada',1.00,'1','2026-10-24 11:15:19');
/*!40000 ALTER TABLE `movimientocaja` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `parametro_negocio`
--

DROP TABLE IF EXISTS `parametro_negocio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parametro_negocio` (
  `idParametro` int NOT NULL AUTO_INCREMENT,
  `precioExamen` decimal(10,2) NOT NULL DEFAULT '250.00',
  `vigenciaBonificacion` int NOT NULL DEFAULT '30',
  `anticipoMinimo` int NOT NULL DEFAULT '50',
  PRIMARY KEY (`idParametro`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `parametro_negocio`
--

LOCK TABLES `parametro_negocio` WRITE;
/*!40000 ALTER TABLE `parametro_negocio` DISABLE KEYS */;
INSERT INTO `parametro_negocio` VALUES (1,250.00,30,60);
/*!40000 ALTER TABLE `parametro_negocio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedido`
--

DROP TABLE IF EXISTS `pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedido` (
  `folioPedido` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `idCotizacion` int NOT NULL,
  `idLaboratorio` int NOT NULL,
  `fechaRegisEp` date NOT NULL,
  `fechaEntrega` date DEFAULT NULL,
  `anticipo` tinyint(1) DEFAULT '0',
  `anticipoTotal` decimal(10,2) DEFAULT NULL,
  `saldoPendiente` decimal(10,2) NOT NULL,
  `estado` varchar(50) NOT NULL,
  `telefono` varchar(10) NOT NULL,
  PRIMARY KEY (`folioPedido`),
  KEY `fk_pedido_cliente` (`idCliente`),
  KEY `fk_cotizacion_pedido` (`idCotizacion`),
  KEY `fk_pedido_laboratorio` (`idLaboratorio`),
  CONSTRAINT `fk_cotizacion_pedido` FOREIGN KEY (`idCotizacion`) REFERENCES `cotizacion` (`idCotizacion`),
  CONSTRAINT `fk_pedido_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_pedido_laboratorio` FOREIGN KEY (`idLaboratorio`) REFERENCES `laboratorio` (`idLaboratorio`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedido`
--

LOCK TABLES `pedido` WRITE;
/*!40000 ALTER TABLE `pedido` DISABLE KEYS */;
/*!40000 ALTER TABLE `pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `producto`
--

DROP TABLE IF EXISTS `producto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `producto` (
  `idProducto` int NOT NULL AUTO_INCREMENT,
  `idClasePrecio` int DEFAULT NULL,
  `nombre` varchar(20) NOT NULL,
  `existencia` int DEFAULT '0',
  `promocion` tinyint(1) DEFAULT '0',
  `precio` decimal(10,2) NOT NULL,
  PRIMARY KEY (`idProducto`),
  KEY `fk_producto_clase` (`idClasePrecio`),
  CONSTRAINT `fk_producto_clase` FOREIGN KEY (`idClasePrecio`) REFERENCES `clase_precio` (`idClasePrecio`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` VALUES (1,NULL,'siiiiiiiiiiiii',5,0,100.00),(2,NULL,'nooooooooo',5,0,100.00),(3,NULL,'NOSE',5,0,100.00);
/*!40000 ALTER TABLE `producto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promocion`
--

DROP TABLE IF EXISTS `promocion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promocion` (
  `idPromocion` int NOT NULL AUTO_INCREMENT,
  `nombrePromocion` varchar(100) DEFAULT NULL,
  `tipoPromocion` varchar(20) DEFAULT NULL,
  `porcentajePromo` decimal(10,2) DEFAULT NULL,
  `fechaInicio` date DEFAULT NULL,
  `fechaFin` date DEFAULT NULL,
  `acumulable` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`idPromocion`),
  CONSTRAINT `chk_valor_promocion` CHECK ((((`tipoPromocion` = _utf8mb4'Porcentaje') and (`porcentajePromo` between 0.1 and 100.00)) or ((`tipoPromocion` in (_utf8mb4'MontoFijo',_utf8mb4'Paquete')) and (`porcentajePromo` between 0.1 and 1000000.00))))
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promocion`
--

LOCK TABLES `promocion` WRITE;
/*!40000 ALTER TABLE `promocion` DISABLE KEYS */;
INSERT INTO `promocion` VALUES (7,'klkl','Porcentaje',45.00,'2026-10-23','2026-10-23',0),(8,'klkl','Porcentaje',87.00,'2027-05-04','2027-06-15',1),(9,'klkl','Porcentaje',20.00,'2026-11-10','2026-11-11',0);
/*!40000 ALTER TABLE `promocion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promocion_producto`
--

DROP TABLE IF EXISTS `promocion_producto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promocion_producto` (
  `idPromocion` int NOT NULL,
  `idProducto` int NOT NULL,
  PRIMARY KEY (`idPromocion`,`idProducto`),
  KEY `fk_pp_producto` (`idProducto`),
  CONSTRAINT `fk_pp_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`) ON DELETE CASCADE,
  CONSTRAINT `fk_pp_promocion` FOREIGN KEY (`idPromocion`) REFERENCES `promocion` (`idPromocion`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promocion_producto`
--

LOCK TABLES `promocion_producto` WRITE;
/*!40000 ALTER TABLE `promocion_producto` DISABLE KEYS */;
/*!40000 ALTER TABLE `promocion_producto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `receta`
--

DROP TABLE IF EXISTS `receta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `receta` (
  `idReceta` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `esfera` varchar(15) DEFAULT NULL,
  `cilindro` varchar(15) DEFAULT NULL,
  `eje` varchar(15) DEFAULT NULL,
  `distanciaPupilar` varchar(15) DEFAULT NULL,
  `fecha` date NOT NULL,
  `vigencia` date NOT NULL,
  `recetaExterna` tinyint(1) DEFAULT '0',
  `emisor` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`idReceta`),
  KEY `fk_receta_cliente` (`idCliente`),
  KEY `fk_receta_empleado` (`idEmpleado`),
  CONSTRAINT `fk_receta_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_receta_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `receta`
--

LOCK TABLES `receta` WRITE;
/*!40000 ALTER TABLE `receta` DISABLE KEYS */;
/*!40000 ALTER TABLE `receta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reparacion`
--

DROP TABLE IF EXISTS `reparacion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reparacion` (
  `folioReparacion` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `fechaRegistro` date NOT NULL,
  `fechaEntrega` date DEFAULT NULL,
  `descripcion` varchar(50) DEFAULT NULL,
  `estadoFalla` varchar(50) DEFAULT NULL,
  `precioEstimado` decimal(10,2) DEFAULT NULL,
  `estadoLentes` varchar(20) DEFAULT NULL,
  `reparacionExito` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`folioReparacion`),
  KEY `fk_reparacion_cliente` (`idCliente`),
  KEY `fk_reparacion_empleado` (`idEmpleado`),
  CONSTRAINT `fk_reparacion_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_reparacion_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reparacion`
--

LOCK TABLES `reparacion` WRITE;
/*!40000 ALTER TABLE `reparacion` DISABLE KEYS */;
/*!40000 ALTER TABLE `reparacion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rol`
--

DROP TABLE IF EXISTS `rol`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rol` (
  `idRol` int NOT NULL AUTO_INCREMENT,
  `nombreRol` varchar(30) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`idRol`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rol`
--

LOCK TABLES `rol` WRITE;
/*!40000 ALTER TABLE `rol` DISABLE KEYS */;
INSERT INTO `rol` VALUES (1,'Encargado','Administrador principal de la óptica'),(2,'Vendedor','Atención en mostrador, ventas y caja'),(3,'Optometrista','Exámenes visuales y generación de recetas'),(4,'Laboratorista','Personal técnico de laboratorio');
/*!40000 ALTER TABLE `rol` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket`
--

DROP TABLE IF EXISTS `ticket`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket` (
  `idTicket` int NOT NULL AUTO_INCREMENT,
  `folioVenta` int NOT NULL,
  PRIMARY KEY (`idTicket`),
  KEY `fk_ticket_venta` (`folioVenta`),
  CONSTRAINT `fk_ticket_venta` FOREIGN KEY (`folioVenta`) REFERENCES `venta` (`folioVenta`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket`
--

LOCK TABLES `ticket` WRITE;
/*!40000 ALTER TABLE `ticket` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `venta`
--

DROP TABLE IF EXISTS `venta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `venta` (
  `folioVenta` int NOT NULL AUTO_INCREMENT,
  `idCliente` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `idProducto` int NOT NULL,
  `fecha` date NOT NULL,
  `formaPago` varchar(20) NOT NULL,
  `total` decimal(10,2) NOT NULL,
  `ExamenComprado` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`folioVenta`),
  KEY `fk_venta_cliente` (`idCliente`),
  KEY `fk_venta_empleado` (`idEmpleado`),
  KEY `fk_venta_producto` (`idProducto`),
  CONSTRAINT `fk_venta_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_venta_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_venta_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `venta`
--

LOCK TABLES `venta` WRITE;
/*!40000 ALTER TABLE `venta` DISABLE KEYS */;
/*!40000 ALTER TABLE `venta` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-24 11:19:49
