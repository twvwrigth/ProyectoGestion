# Reglas de desarrollo en equipo (Scrum / Git)

1. **Aislamiento de código:** Solo debes sugerir o realizar modificaciones en los archivos explícitamente mencionados o relacionados con la historia de usuario asignada.
2. **Confirmación previa:** NUNCA sobrescribas un archivo automáticamente. Muestra el código propuesto en el chat o en diff y PREGUNTA antes de aplicar cualquier cambio en el repositorio.
3. **Conflictos con el equipo:** Si detectas que un cambio afecta un modelo, servicio o archivo compartido por otros compañeros de equipo, ADVIERTE de inmediato y no procedas hasta confirmar.
4. **Base de Datos:** Para consultas o migraciones de base de datos, genera los scripts .sql necesarios pero NO ejecutes consultas DDL/DML destructivas en la base de datos sin confirmación.