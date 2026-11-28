CREATE DATABASE  IF NOT EXISTS `dboptica` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `dboptica`;
-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: dboptica
-- ------------------------------------------------------
-- Server version	8.0.43

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
-- Table structure for table `ajuste_inventario`
--

DROP TABLE IF EXISTS `ajuste_inventario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ajuste_inventario` (
  `idAjuste` int NOT NULL AUTO_INCREMENT,
  `idProducto` int NOT NULL,
  `cantidadAntes` int NOT NULL,
  `cantidadDespues` int NOT NULL,
  `tipoAjuste` varchar(50) NOT NULL,
  `motivo` varchar(255) NOT NULL,
  `idEmpleado` int NOT NULL,
  `idEncargadoAutoriza` int DEFAULT NULL,
  `fecha` datetime NOT NULL,
  `idAutorizacion` int DEFAULT NULL,
  PRIMARY KEY (`idAjuste`),
  KEY `fk_aju_producto` (`idProducto`),
  KEY `fk_aju_empleado` (`idEmpleado`),
  KEY `fk_aju_encargado` (`idEncargadoAutoriza`),
  KEY `fk_aju_autorizacion` (`idAutorizacion`),
  CONSTRAINT `fk_aju_autorizacion` FOREIGN KEY (`idAutorizacion`) REFERENCES `autorizacion` (`idAutorizacion`),
  CONSTRAINT `fk_aju_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_aju_encargado` FOREIGN KEY (`idEncargadoAutoriza`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_aju_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ajuste_inventario`
--

LOCK TABLES `ajuste_inventario` WRITE;
/*!40000 ALTER TABLE `ajuste_inventario` DISABLE KEYS */;
/*!40000 ALTER TABLE `ajuste_inventario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `autorizacion`
--

DROP TABLE IF EXISTS `autorizacion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `autorizacion` (
  `idAutorizacion` int NOT NULL AUTO_INCREMENT,
  `tipoAccion` varchar(100) NOT NULL,
  `folioReferencia` varchar(50) DEFAULT NULL,
  `idEmpleadoSolicita` int NOT NULL,
  `idEncargadoAutoriza` int DEFAULT NULL,
  `fechaSolicitud` datetime NOT NULL,
  `fechaResolucion` datetime DEFAULT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'Pendiente',
  `motivo` varchar(255) NOT NULL,
  `detalle` varchar(255) DEFAULT NULL,
  `usada` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`idAutorizacion`),
  KEY `fk_aut_solicita` (`idEmpleadoSolicita`),
  KEY `fk_aut_encargado` (`idEncargadoAutoriza`),
  CONSTRAINT `fk_aut_encargado` FOREIGN KEY (`idEncargadoAutoriza`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_aut_solicita` FOREIGN KEY (`idEmpleadoSolicita`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `autorizacion`
--

LOCK TABLES `autorizacion` WRITE;
/*!40000 ALTER TABLE `autorizacion` DISABLE KEYS */;
/*!40000 ALTER TABLE `autorizacion` ENABLE KEYS */;
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
  `fecha_hora` datetime DEFAULT NULL,
  `tipo_accion` varchar(100) DEFAULT NULL,
  `estado` varchar(20) DEFAULT NULL,
  `folio_referencia` varchar(50) DEFAULT NULL,
  `motivo` varchar(255) DEFAULT NULL,
  `cambioRealizado` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`idReporte`),
  KEY `fk_bitacora_empleado` (`idEmpleado`),
  KEY `fk_bitacora_encargado` (`idEncargado`),
  CONSTRAINT `fk_bitacora_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_bitacora_encargado` FOREIGN KEY (`idEncargado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bitacora`
--

LOCK TABLES `bitacora` WRITE;
/*!40000 ALTER TABLE `bitacora` DISABLE KEYS */;
INSERT INTO `bitacora` VALUES (1,3,1,'2026-11-12','2026-11-12 12:00:00','RegistroHistorico','Aprobada',NULL,'Autorización de descuento especial a cliente frecuente','Se aplicó bonificación completa del examen visual en cotización #4'),(2,2,1,'2026-11-16','2026-11-16 12:00:00','RegistroHistorico','Aprobada',NULL,'Incidencia reportada por laboratorio externo en pedido #3','Se cambió estado del pedido #3 a Con incidencia para reposición de mica');
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cajaturno`
--

LOCK TABLES `cajaturno` WRITE;
/*!40000 ALTER TABLE `cajaturno` DISABLE KEYS */;
INSERT INTO `cajaturno` VALUES (1,2,'2026-11-17 09:00:00',1500.00,'Cerrada','2026-11-17 19:00:00',2240.00,2240.00,0.00,NULL),(2,2,'2026-11-18 09:00:00',1500.00,'Cerrada','2026-11-18 16:18:48',16335.00,16335.00,0.00,NULL),(3,1,'2026-11-18 16:18:51',0.00,'Abierta',NULL,NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `cajaturno` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `categoria`
--

DROP TABLE IF EXISTS `categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categoria` (
  `idCategoria` int NOT NULL AUTO_INCREMENT,
  `nombreCategoria` varchar(50) NOT NULL,
  `camposRequeridos` json DEFAULT NULL,
  PRIMARY KEY (`idCategoria`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categoria`
--

LOCK TABLES `categoria` WRITE;
/*!40000 ALTER TABLE `categoria` DISABLE KEYS */;
INSERT INTO `categoria` VALUES (1,'Armazones Oftálmicos','[\"Marca\", \"Material\", \"Color\", \"TipoAro\"]'),(2,'Lentes de Sol','[\"Marca\", \"Polarizado\", \"FiltroUV\", \"Color\"]'),(3,'Lentes de Contacto','[\"Marca\", \"Duracion\", \"ContenidoCaja\"]'),(4,'Soluciones y Gotas','[\"Marca\", \"ContenidoML\", \"Funcion\"]'),(5,'Accesorios','[\"Material\", \"Color\"]');
/*!40000 ALTER TABLE `categoria` ENABLE KEYS */;
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clase_precio`
--

LOCK TABLES `clase_precio` WRITE;
/*!40000 ALTER TABLE `clase_precio` DISABLE KEYS */;
INSERT INTO `clase_precio` VALUES (1,'Económica',500,1200),(2,'Intermedia',1201,2800),(3,'Premium',2801,7500);
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
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cliente`
--

LOCK TABLES `cliente` WRITE;
/*!40000 ALTER TABLE `cliente` DISABLE KEYS */;
INSERT INTO `cliente` VALUES (1,'Alejandro Ramírez Cruz','2291457890',1),(2,'Mariana Delgado Soto','2292345612',1),(3,'José Luis Hernández','2298765432',1),(4,'Patricia Navarro Gil','2293456789',1),(5,'Fernando Castillo Ruiz','2294567890',1),(6,'Valeria Peña Mendoza','2295678901',1),(7,'Ricardo Ochoa Salinas','2296789012',1),(8,'Gabriela Torres León','2297890123',1);
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
  `detalleCotizacion` varchar(500) DEFAULT NULL,
  `descuento` decimal(10,2) NOT NULL DEFAULT '0.00',
  `idVentaExamenBonificado` int DEFAULT NULL,
  PRIMARY KEY (`idCotizacion`),
  KEY `fk_cotizacion_cliente` (`idCliente`),
  KEY `fk_cotizacion_empleado` (`idEmpleado`),
  KEY `fk_cotizacion_receta` (`idReceta`),
  KEY `fk_cot_examen_credito` (`idVentaExamenBonificado`),
  CONSTRAINT `fk_cot_examen_credito` FOREIGN KEY (`idVentaExamenBonificado`) REFERENCES `venta` (`folioVenta`) ON DELETE SET NULL,
  CONSTRAINT `fk_cotizacion_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_cotizacion_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_cotizacion_receta` FOREIGN KEY (`idReceta`) REFERENCES `receta` (`idReceta`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cotizacion`
--

LOCK TABLES `cotizacion` WRITE;
/*!40000 ALTER TABLE `cotizacion` DISABLE KEYS */;
INSERT INTO `cotizacion` VALUES (1,1,2,1,'2026-11-05',4600.00,'2026-11-20',1,1,'Armazón Ray-Ban Clubmaster ($3,200) + Micas Policarbonato Luz Azul ($1,650) - Bonificación examen ($250)',0.00,NULL),(2,2,2,2,'2026-11-07',5100.00,'2026-11-22',1,1,'Armazón Oakley Holbrook Rx ($2,450) + Micas Transitions Gen S ($2,900) - Bonificación examen ($250)',0.00,NULL),(3,3,3,3,'2026-11-10',3400.00,'2026-11-25',0,1,'Armazón propio del cliente + Micas High Index 1.67 ($3,400)',0.00,NULL),(4,4,2,4,'2026-11-12',1800.00,'2026-11-27',1,1,'Armazón Vogue Classic Slim ($1,100) + Micas CR-39 AR ($950) - Bonificación examen ($250)',0.00,NULL),(5,5,3,5,'2026-11-14',2750.00,'2026-11-29',0,1,'Armazón Vogue Classic Slim ($1,100) + Micas Policarbonato Luz Azul ($1,650)',0.00,NULL),(6,6,2,6,'2026-11-16',5850.00,'2026-12-01',1,1,'Armazón Ray-Ban Clubmaster ($3,200) + Micas Transitions Gen S ($2,900) - Bonificación examen ($250)',0.00,NULL),(7,5,1,5,'2026-11-18',4720.00,'2026-12-03',1,1,'Mica (RECOMENDADA) - Policarbonato - Antirreflejante + Filtro Luz Azul | Ray-Ban Clubmaster | Kit Limpieza Microf.',0.00,NULL),(8,1,1,1,'2026-11-24',700.00,'2026-12-09',1,1,'Mica (MINIMA) - CR-39 - Antirreflejante Básico',0.00,NULL),(9,8,1,7,'2026-11-27',3270.00,'2026-12-12',1,1,'Mica (RECOMENDADA) - High Index 1.67 - Antirreflejante Hidrofóbico + Luz Azul | Kit Limpieza Microf.',0.00,NULL),(10,5,1,5,'2026-11-27',1400.00,'2026-12-12',1,1,'Mica (RECOMENDADA) - Policarbonato - Antirreflejante + Filtro Luz Azul',0.00,NULL),(11,3,1,3,'2026-11-27',3400.00,'2026-12-12',0,1,'Mica (RECOMENDADA) - High Index 1.67 - Antirreflejante Hidrofóbico + Luz Azul',0.00,NULL);
/*!40000 ALTER TABLE `cotizacion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `detalle_venta`
--

DROP TABLE IF EXISTS `detalle_venta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `detalle_venta` (
  `idDetalleVenta` int NOT NULL AUTO_INCREMENT,
  `folioVenta` int NOT NULL,
  `idProducto` int NOT NULL,
  `cantidad` int NOT NULL,
  `precioUnitario` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) NOT NULL,
  `descuento` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`idDetalleVenta`),
  UNIQUE KEY `uq_detalle_venta` (`folioVenta`,`idProducto`),
  KEY `fk_dv_venta` (`folioVenta`),
  KEY `fk_dv_producto` (`idProducto`),
  CONSTRAINT `fk_dv_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_dv_venta` FOREIGN KEY (`folioVenta`) REFERENCES `venta` (`folioVenta`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `chk_dv_cantidad` CHECK ((`cantidad` > 0)),
  CONSTRAINT `chk_dv_precio` CHECK ((`precioUnitario` >= 0)),
  CONSTRAINT `chk_dv_subtotal` CHECK ((`subtotal` = (`cantidad` * `precioUnitario`)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_venta`
--

LOCK TABLES `detalle_venta` WRITE;
/*!40000 ALTER TABLE `detalle_venta` DISABLE KEYS */;
/*!40000 ALTER TABLE `detalle_venta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `devolucion`
--

DROP TABLE IF EXISTS `devolucion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `devolucion` (
  `idDevolucion` int NOT NULL AUTO_INCREMENT,
  `folioVenta` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `idEncargadoAutoriza` int DEFAULT NULL,
  `fecha` datetime NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `cantidadDevuelta` int NOT NULL DEFAULT '1',
  `formaPagoDevolucion` varchar(20) NOT NULL,
  `motivo` varchar(255) NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'Completada',
  `idAutorizacion` int DEFAULT NULL,
  PRIMARY KEY (`idDevolucion`),
  KEY `fk_dev_venta` (`folioVenta`),
  KEY `fk_dev_empleado` (`idEmpleado`),
  KEY `fk_dev_encargado` (`idEncargadoAutoriza`),
  KEY `fk_dev_autorizacion` (`idAutorizacion`),
  CONSTRAINT `fk_dev_autorizacion` FOREIGN KEY (`idAutorizacion`) REFERENCES `autorizacion` (`idAutorizacion`),
  CONSTRAINT `fk_dev_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_dev_encargado` FOREIGN KEY (`idEncargadoAutoriza`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_dev_venta` FOREIGN KEY (`folioVenta`) REFERENCES `venta` (`folioVenta`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `devolucion`
--

LOCK TABLES `devolucion` WRITE;
/*!40000 ALTER TABLE `devolucion` DISABLE KEYS */;
/*!40000 ALTER TABLE `devolucion` ENABLE KEYS */;
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
  `bloqueadoHasta` datetime DEFAULT NULL,
  PRIMARY KEY (`idEmpleado`),
  UNIQUE KEY `usuario` (`usuario`),
  KEY `fk_empleado_rol` (`idRol`),
  KEY `fk_empleado_laboratorio` (`idLaboratorio`),
  CONSTRAINT `fk_empleado_laboratorio` FOREIGN KEY (`idLaboratorio`) REFERENCES `laboratorio` (`idLaboratorio`),
  CONSTRAINT `fk_empleado_rol` FOREIGN KEY (`idRol`) REFERENCES `rol` (`idRol`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleado`
--

LOCK TABLES `empleado` WRITE;
/*!40000 ALTER TABLE `empleado` DISABLE KEYS */;
INSERT INTO `empleado` VALUES (1,1,NULL,'Luis David Sosa Fernandez','sosa','admin123',1,0,NULL),(2,2,NULL,'Laura Sánchez','lsanchez','Venta123',1,0,NULL),(3,2,NULL,'Diego Morales','dmorales','Venta123',1,0,NULL),(4,3,NULL,'Dra. Fernanda Ortiz','fortiz','Opto123',1,0,NULL),(5,4,1,'Carlos Domínguez','cdominguez','Lab123',1,0,NULL),(6,4,2,'Héctor Vargas','hvargas','Lab123',1,0,NULL),(7,4,1,'Kendar','kakendar@sosacorp.com','contrasenakakendar@sosacorp.com',1,0,NULL),(8,4,1,'sdfsdf','sdfsd@sfsd','contrasenasd',0,0,NULL),(9,4,1,'sdfsdffsdfg','sdfs@ssdsd','contrasenasdfs@ssdsd',0,0,NULL),(10,4,1,'sdfsdfsdf','sdfsdfs@sdfsdf','contrasenasdfsdfs@sdfsdf',0,0,NULL),(11,4,1,'sdfsdfsdfsdqs','sdfsdfsdfsd@sdfsdf','contrasenasdfsdfsdfsd@sdfsdf',0,0,NULL),(12,4,1,'sdfsssssssssssssssssssssssssssssssssss','sdfsdf@asdfsdf','contrasenasdfsdf@asdfsdf',0,0,NULL);
/*!40000 ALTER TABLE `empleado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historial_pedido`
--

DROP TABLE IF EXISTS `historial_pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historial_pedido` (
  `idHistorial` int NOT NULL AUTO_INCREMENT,
  `folioPedido` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `estado` varchar(50) NOT NULL,
  `fechaCambio` datetime NOT NULL,
  `comentario` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`idHistorial`),
  KEY `fk_historial_pedido` (`folioPedido`),
  KEY `fk_historial_empleado` (`idEmpleado`),
  CONSTRAINT `fk_historial_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_historial_pedido` FOREIGN KEY (`folioPedido`) REFERENCES `pedido` (`folioPedido`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historial_pedido`
--

LOCK TABLES `historial_pedido` WRITE;
/*!40000 ALTER TABLE `historial_pedido` DISABLE KEYS */;
INSERT INTO `historial_pedido` VALUES (1,1,2,'Registrado','2026-11-05 11:00:00','Pedido registrado con 50% de anticipo ($2,300.00).'),(2,1,5,'En proceso','2026-11-06 09:15:00','Biselado y montaje iniciado en taller propio.'),(3,1,5,'Terminado','2026-11-08 16:30:00','Control de calidad aprobado.'),(4,1,2,'Entregado','2026-11-10 17:20:00','Cliente liquidó saldo de $2,300.00 y recibió sus lentes.'),(5,2,2,'Registrado','2026-11-07 12:10:00','Anticipo del 50% cobrado.'),(6,2,2,'Enviado a laboratorio','2026-11-07 17:00:00','Enviado por mensajería a Essilor Veracruz.'),(7,2,2,'Recibido en óptica','2026-11-14 13:00:00','Recibido de laboratorio externo, graduación verificada.'),(8,2,2,'Listo para entrega','2026-11-14 13:20:00','Se notificó al cliente vía WhatsApp.'),(9,3,3,'Registrado','2026-11-10 10:30:00','Se recibe armazón del cliente en buen estado.'),(10,3,3,'Enviado a laboratorio','2026-11-10 16:00:00','Enviado para tallado High Index 1.67.'),(11,3,1,'Con incidencia','2026-11-16 11:45:00','Laboratorio reporta falla en tratamiento AR; se repite mica sin costo.'),(12,4,2,'Registrado','2026-11-12 14:00:00','Pedido liquidado al 100% desde mostrador.'),(13,4,5,'Terminado','2026-11-17 18:00:00','Montaje finalizado en taller central.'),(14,5,3,'Registrado','2026-11-14 11:20:00','Anticipo de $1,500.00 registrado.'),(15,5,3,'En proceso','2026-11-15 10:00:00','En tallado de oblea bifocal en Óptica Industrial del Golfo.'),(16,6,2,'Registrado','2026-11-16 16:10:00','Anticipo recibido. Pendiente de envío a laboratorio.'),(17,10,5,'En proceso','2026-11-27 17:25:21','Producción actualizada a: En proceso'),(18,11,5,'En proceso','2026-11-27 19:16:07','Producción actualizada a: En proceso'),(19,11,5,'Terminado','2026-11-27 19:17:20','Producción actualizada a: Terminado'),(20,8,5,'En proceso','2026-11-27 19:32:05','Producción actualizada a: En proceso');
/*!40000 ALTER TABLE `historial_pedido` ENABLE KEYS */;
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventario`
--

LOCK TABLES `inventario` WRITE;
/*!40000 ALTER TABLE `inventario` DISABLE KEYS */;
INSERT INTO `inventario` VALUES (1,1,'Luxottica México','2026-10-15',10),(2,2,'Luxottica México','2026-10-15',15),(3,3,'Distribuidora Óptica MX','2026-10-20',20),(4,6,'Bausch + Lomb Distrib.','2026-11-01',30),(5,8,'Insumos Ópticos del Sur','2026-11-05',50);
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `laboratorio`
--

LOCK TABLES `laboratorio` WRITE;
/*!40000 ALTER TABLE `laboratorio` DISABLE KEYS */;
INSERT INTO `laboratorio` VALUES (1,'Taller Óptico Central (Propio)','2299314500','1 a 2 días hábiles','Interno',1),(2,'Laboratorios Essilor Veracruz','2299328810','4 a 6 días hábiles','Externo',1),(3,'Óptica Industrial del Golfo','2299351290','3 a 5 días hábiles','Externo',1);
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lista_espera`
--

LOCK TABLES `lista_espera` WRITE;
/*!40000 ALTER TABLE `lista_espera` DISABLE KEYS */;
INSERT INTO `lista_espera` VALUES (1,1,'2026-11-18 09:30:00',1),(2,2,'2026-11-18 10:15:00',1),(5,8,'2026-11-18 15:58:13',1);
/*!40000 ALTER TABLE `lista_espera` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `micas`
--

DROP TABLE IF EXISTS `micas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `micas` (
  `IdMica` int NOT NULL AUTO_INCREMENT,
  `Material` varchar(100) NOT NULL,
  `Tratamiento` varchar(100) DEFAULT NULL,
  `RangoGraduacion` varchar(100) DEFAULT NULL,
  `Precio` decimal(18,2) NOT NULL,
  PRIMARY KEY (`IdMica`),
  UNIQUE KEY `uq_mica` (`Material`,`Tratamiento`,`RangoGraduacion`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `micas`
--

LOCK TABLES `micas` WRITE;
/*!40000 ALTER TABLE `micas` DISABLE KEYS */;
INSERT INTO `micas` VALUES (1,'CR-39','Blanco (Sin tratamiento)','Esf -4.00 a +4.00 / Cil -2.00',650.00),(2,'CR-39','Antirreflejante Básico','Esf -4.00 a +4.00 / Cil -2.00',950.00),(3,'Policarbonato','Antirreflejante + Filtro Luz Azul','Esf -6.00 a +6.00 / Cil -3.00',1650.00),(4,'Policarbonato','Fotocromático Transitions Gen S','Esf -6.00 a +6.00 / Cil -3.00',2900.00),(5,'High Index 1.67','Antirreflejante Hidrofóbico + Luz Azul','Esf -10.00 a +8.00 / Cil -4.00',3400.00),(6,'Policarbonato','Filtro Luz Azul','6 - 8',400.00);
/*!40000 ALTER TABLE `micas` ENABLE KEYS */;
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `movimientocaja`
--

LOCK TABLES `movimientocaja` WRITE;
/*!40000 ALTER TABLE `movimientocaja` DISABLE KEYS */;
INSERT INTO `movimientocaja` VALUES (1,1,'Salida',150.00,'Pago de mensajería local para envío de pedidos a laboratorio','2026-11-17 13:20:00'),(2,1,'Entrada',500.00,'Reposición de morralla para cambio en mostrador','2026-11-17 15:00:00'),(3,2,'Salida',85.00,'Compra de agua embotellada e insumos de limpieza','2026-11-18 11:30:00');
/*!40000 ALTER TABLE `movimientocaja` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pago_venta`
--

DROP TABLE IF EXISTS `pago_venta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pago_venta` (
  `idPago` int NOT NULL AUTO_INCREMENT,
  `folioVenta` int NOT NULL,
  `formaPago` varchar(20) NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `referencia` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`idPago`),
  KEY `fk_pago_venta` (`folioVenta`),
  CONSTRAINT `fk_pago_venta` FOREIGN KEY (`folioVenta`) REFERENCES `venta` (`folioVenta`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pago_venta`
--

LOCK TABLES `pago_venta` WRITE;
/*!40000 ALTER TABLE `pago_venta` DISABLE KEYS */;
/*!40000 ALTER TABLE `pago_venta` ENABLE KEYS */;
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
  `limiteDescuentoVendedor` decimal(5,2) NOT NULL DEFAULT '10.00',
  PRIMARY KEY (`idParametro`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `parametro_negocio`
--

LOCK TABLES `parametro_negocio` WRITE;
/*!40000 ALTER TABLE `parametro_negocio` DISABLE KEYS */;
INSERT INTO `parametro_negocio` VALUES (1,250.00,30,50,10.00);
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
  `fechaPromesa` date DEFAULT NULL,
  `fechaEntregaReal` datetime DEFAULT NULL,
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
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedido`
--

LOCK TABLES `pedido` WRITE;
/*!40000 ALTER TABLE `pedido` DISABLE KEYS */;
INSERT INTO `pedido` VALUES (1,1,1,1,'2026-11-05','2026-11-10','2026-11-10 17:20:00',1,2300.00,0.00,'Entregado','2291457890'),(2,2,2,2,'2026-11-07','2026-11-15',NULL,1,2550.00,2550.00,'Listo para entrega','2292345612'),(3,3,3,2,'2026-11-10','2026-11-17',NULL,1,2000.00,1400.00,'Con incidencia','2298765432'),(4,4,4,1,'2026-11-12','2026-11-19',NULL,1,1800.00,0.00,'Entregado','2293456789'),(5,5,5,3,'2026-11-14','2026-11-20',NULL,1,1500.00,1250.00,'Recibido en óptica','2294567890'),(6,6,6,2,'2026-11-16','2026-11-24',NULL,1,3000.00,2850.00,'Registrado','2295678901'),(7,5,7,1,'2026-11-18','2026-11-25',NULL,0,NULL,4720.00,'Registrado','2294567890'),(8,1,8,1,'2026-11-24','2026-12-01',NULL,0,NULL,700.00,'En proceso','2291457890'),(9,8,9,1,'2026-11-27','2026-12-04',NULL,0,NULL,0.00,'Entregado','2297890123'),(10,5,10,1,'2026-11-27','2026-12-04',NULL,0,NULL,0.00,'Entregado','2294567890'),(11,3,11,1,'2026-11-27','2026-12-04',NULL,1,1700.00,1700.00,'Recibido en óptica','2298765432');
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
  `idCategoria` int NOT NULL DEFAULT '1',
  `idClasePrecio` int DEFAULT NULL,
  `nombre` varchar(20) NOT NULL,
  `existencia` int DEFAULT '0',
  `promocion` tinyint(1) DEFAULT '0',
  `precio` decimal(10,2) NOT NULL,
  `detallesTecnicos` json DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_baja` datetime DEFAULT NULL,
  PRIMARY KEY (`idProducto`),
  KEY `fk_producto_clase` (`idClasePrecio`),
  KEY `fk_producto_categoria` (`idCategoria`),
  CONSTRAINT `fk_producto_categoria` FOREIGN KEY (`idCategoria`) REFERENCES `categoria` (`idCategoria`),
  CONSTRAINT `fk_producto_clase` FOREIGN KEY (`idClasePrecio`) REFERENCES `clase_precio` (`idClasePrecio`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` VALUES (1,1,3,'Ray-Ban Clubmaster',5,1,3200.00,'{\"Color\": \"Carey/Dorado\", \"Marca\": \"Ray-Ban\", \"TipoAro\": \"Semi al aire\", \"Material\": \"Acetato/Metal\"}',1,NULL),(2,1,2,'Oakley Holbrook Rx',11,0,2450.00,'{\"Color\": \"Negro Mate\", \"Marca\": \"Oakley\", \"TipoAro\": \"Aro completo\", \"Material\": \"O-Matter\"}',1,NULL),(3,1,1,'Vogue Classic Slim',14,1,1100.00,'{\"Color\": \"Oro Rosa\", \"Marca\": \"Vogue\", \"TipoAro\": \"Aro completo\", \"Material\": \"Metal\"}',1,NULL),(4,2,3,'Ray-Ban Aviator Sol',4,0,3500.00,'{\"Color\": \"Dorado/Verde\", \"Marca\": \"Ray-Ban\", \"FiltroUV\": \"UV400\", \"Polarizado\": \"Sí\"}',1,NULL),(5,3,2,'Acuvue Oasys 6pz',19,0,890.00,'{\"Marca\": \"Johnson & Johnson\", \"Duracion\": \"Quincenal\", \"ContenidoCaja\": \"6 lentes\"}',1,NULL),(6,4,1,'Solución Renu 355ml',24,1,220.00,'{\"Marca\": \"Bausch + Lomb\", \"Funcion\": \"Limpieza y desinfección\", \"ContenidoML\": \"355 ml\"}',1,NULL),(7,4,1,'Gotas Systane Ultra',17,0,340.00,'{\"Marca\": \"Alcon\", \"Funcion\": \"Lubricante ocular\", \"ContenidoML\": \"10 ml\"}',1,NULL),(8,5,1,'Kit Limpieza Microf.',39,0,120.00,'{\"Color\": \"Azul\", \"Material\": \"Microfibra y spray\"}',1,NULL),(9,1,NULL,'Liquidación pedido',0,0,0.00,NULL,1,NULL),(10,1,NULL,'Anticipo pedido',0,0,0.00,NULL,1,NULL);
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promocion`
--

LOCK TABLES `promocion` WRITE;
/*!40000 ALTER TABLE `promocion` DISABLE KEYS */;
INSERT INTO `promocion` VALUES (1,'Buen Fin Armazones Seleccionados','Porcentaje',20.00,'2026-11-10','2026-11-25',0);
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
INSERT INTO `promocion_producto` VALUES (1,1),(1,3);
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
  `distanciaPupilar` varchar(15) DEFAULT NULL,
  `fecha` date NOT NULL,
  `vigencia` date NOT NULL,
  `recetaExterna` tinyint(1) DEFAULT '0',
  `emisor` varchar(50) DEFAULT NULL,
  `esferaOD` varchar(15) DEFAULT NULL,
  `cilindroOD` varchar(15) DEFAULT NULL,
  `ejeOD` varchar(15) DEFAULT NULL,
  `adicionOD` varchar(15) DEFAULT NULL,
  `esferaOI` varchar(15) DEFAULT NULL,
  `cilindroOI` varchar(15) DEFAULT NULL,
  `ejeOI` varchar(15) DEFAULT NULL,
  `adicionOI` varchar(15) DEFAULT NULL,
  `altura` varchar(15) DEFAULT NULL,
  `tipoMica` varchar(50) DEFAULT NULL,
  `micaMinima` int DEFAULT NULL,
  `micaRecomendada` int DEFAULT NULL,
  PRIMARY KEY (`idReceta`),
  KEY `fk_receta_cliente` (`idCliente`),
  KEY `fk_receta_empleado` (`idEmpleado`),
  KEY `fk_receta_mica_minima` (`micaMinima`),
  KEY `fk_receta_mica_recomendada` (`micaRecomendada`),
  CONSTRAINT `fk_receta_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_receta_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_receta_mica_minima` FOREIGN KEY (`micaMinima`) REFERENCES `micas` (`IdMica`),
  CONSTRAINT `fk_receta_mica_recomendada` FOREIGN KEY (`micaRecomendada`) REFERENCES `micas` (`IdMica`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `receta`
--

LOCK TABLES `receta` WRITE;
/*!40000 ALTER TABLE `receta` DISABLE KEYS */;
INSERT INTO `receta` VALUES (1,1,4,'64 mm','2026-11-05','2027-11-05',0,NULL,'-1.75','-0.50','180',NULL,'-2.00','-0.75','175',NULL,'18 mm','Monofocal',2,3),(2,2,4,'61 mm','2026-11-07','2027-11-07',0,NULL,'+1.25','-0.50','90','+1.75','+1.50','-0.25','85','+1.75','21 mm','Progresivo',3,4),(3,3,4,'65 mm','2026-11-10','2027-11-10',1,'Hospital Regional Alta Especialidad','-5.50','-1.25','10',NULL,'-6.00','-1.50','170',NULL,'19 mm','Monofocal',3,5),(4,4,4,'62 mm','2026-11-12','2027-11-12',0,NULL,'-0.75',NULL,NULL,NULL,'-1.00','-0.25','160',NULL,'17 mm','Monofocal',1,2),(5,5,4,'63 mm','2026-11-14','2027-11-14',0,NULL,'-2.50','-1.00','180','+1.50','-2.25','-1.25','180','+1.50','20 mm','Bifocal Flat-Top',2,3),(6,6,4,'60 mm','2026-11-16','2027-11-16',0,NULL,'-1.25','-0.50','95',NULL,'-1.25','-0.50','85',NULL,'18 mm','Monofocal',2,4),(7,8,1,'30','2026-11-18','2027-11-18',0,'Óptica (Interno)','2','2','2','2','2','2','2','2','30',NULL,3,5);
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
  `estadoLentes` varchar(100) DEFAULT NULL,
  `reparacionExito` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`folioReparacion`),
  KEY `fk_reparacion_cliente` (`idCliente`),
  KEY `fk_reparacion_empleado` (`idEmpleado`),
  CONSTRAINT `fk_reparacion_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_reparacion_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reparacion`
--

LOCK TABLES `reparacion` WRITE;
/*!40000 ALTER TABLE `reparacion` DISABLE KEYS */;
INSERT INTO `reparacion` VALUES (1,3,2,'2026-11-12','2026-11-14','Armazón metálico dorado marca Carrera','Soldadura de puente nasal fracturado',180.00,'Micas rayadas por uso previo, sin plaquetas',1),(2,7,3,'2026-11-17','2026-11-19','Armazón de acetato negro Hugo Boss','Cambio de tornillos y ajuste de varillas',80.00,'Buen estado general, varilla derecha suelta',NULL);
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
INSERT INTO `rol` VALUES (1,'Encargado','Administrador general de la óptica, cortes, precios y personal'),(2,'Vendedor','Atención en mostrador, cotizaciones, ventas, pedidos y caja'),(3,'Optometrista','Realización de exámenes visuales y emisión de recetas'),(4,'Laboratorista','Tallado, biselado, montaje de micas y control de calidad');
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket`
--

LOCK TABLES `ticket` WRITE;
/*!40000 ALTER TABLE `ticket` DISABLE KEYS */;
INSERT INTO `ticket` VALUES (1,1),(2,2),(3,3),(4,4),(5,5);
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
  `idCajaTurno` int DEFAULT NULL,
  `idPedido` int DEFAULT NULL,
  `idCliente` int NOT NULL,
  `idEmpleado` int NOT NULL,
  `idProducto` int NOT NULL,
  `fecha` date NOT NULL,
  `formaPago` varchar(20) NOT NULL,
  `total` decimal(10,2) NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'Activa',
  `cantidad` int NOT NULL DEFAULT '1',
  `descuento` decimal(10,2) NOT NULL DEFAULT '0.00',
  `efectivoRecibido` decimal(10,2) DEFAULT NULL,
  `cambio` decimal(10,2) DEFAULT NULL,
  `creditoExamenUsado` tinyint(1) NOT NULL DEFAULT '0',
  `precio_venta` decimal(10,2) DEFAULT NULL,
  `motivo_cancelacion` varchar(255) DEFAULT NULL,
  `fecha_cancelacion` datetime DEFAULT NULL,
  `ExamenComprado` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`folioVenta`),
  KEY `fk_venta_cliente` (`idCliente`),
  KEY `fk_venta_empleado` (`idEmpleado`),
  KEY `fk_venta_producto` (`idProducto`),
  KEY `fk_venta_caja` (`idCajaTurno`),
  KEY `fk_venta_pedido` (`idPedido`),
  CONSTRAINT `fk_venta_caja` FOREIGN KEY (`idCajaTurno`) REFERENCES `cajaturno` (`idCajaTurno`),
  CONSTRAINT `fk_venta_cliente` FOREIGN KEY (`idCliente`) REFERENCES `cliente` (`idCliente`),
  CONSTRAINT `fk_venta_empleado` FOREIGN KEY (`idEmpleado`) REFERENCES `empleado` (`idEmpleado`),
  CONSTRAINT `fk_venta_pedido` FOREIGN KEY (`idPedido`) REFERENCES `pedido` (`folioPedido`),
  CONSTRAINT `fk_venta_producto` FOREIGN KEY (`idProducto`) REFERENCES `producto` (`idProducto`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `venta`
--

LOCK TABLES `venta` WRITE;
/*!40000 ALTER TABLE `venta` DISABLE KEYS */;
INSERT INTO `venta` VALUES (1,1,NULL,4,2,6,'2026-11-17','Efectivo',180.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(2,1,NULL,6,2,7,'2026-11-17','Efectivo',340.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(3,1,NULL,1,2,4,'2026-11-17','Tarjeta',3500.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(4,2,NULL,2,2,8,'2026-11-18','Efectivo',120.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(5,2,NULL,5,3,5,'2026-11-18','Transferencia',890.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(6,2,NULL,5,2,4,'2026-11-18','Efectivo',3500.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(7,2,NULL,2,2,1,'2026-11-18','Efectivo',11300.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(8,3,NULL,5,2,1,'2026-11-18','Efectivo',5650.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(9,3,NULL,1,1,1,'2026-11-24','Efectivo',3200.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(10,3,NULL,1,1,2,'2026-11-24','Efectivo',2450.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(11,3,NULL,1,1,3,'2026-11-24','Efectivo',1100.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(12,3,NULL,1,1,4,'2026-11-24','Efectivo',3500.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(13,3,NULL,1,1,5,'2026-11-24','Efectivo',890.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(14,3,NULL,1,1,6,'2026-11-24','Efectivo',220.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(15,3,NULL,1,1,7,'2026-11-24','Efectivo',340.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(16,3,NULL,1,1,8,'2026-11-24','Efectivo',120.00,'0',0,1.00,0.00,NULL,0,NULL,NULL,NULL,0),(17,3,9,8,1,9,'2026-11-27','Efectivo',3270.00,'Activa',1,0.00,NULL,NULL,0,NULL,NULL,NULL,0),(18,3,10,5,1,9,'2026-11-27','Tarjeta',1400.00,'Activa',1,0.00,NULL,NULL,0,NULL,NULL,NULL,0),(19,3,11,3,1,10,'2026-11-27','Efectivo',1700.00,'Activa',1,0.00,NULL,NULL,0,NULL,NULL,NULL,0);
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

-- Dump completed on 2026-11-27 19:35:32
