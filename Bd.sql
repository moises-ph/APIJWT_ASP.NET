create database DBFACTURA;

use DBFACTURA;

create table Users(
	Id int identity(100,1) primary key,
	Name varchar(50) not null,
	Lastname varchar(50) not null,
	email varchar(100) not null unique,
	password varchar(max) not null
);

select * from Users
-- Procedimientos

go
create procedure Auth_usr
	@email varchar(100)
as
begin
	SELECT password, Id from Users where email = @email;
end

go
create procedure Select_usr
	@Id int
as
begin
	SELECT Name, Lastname, email from Users where Id = @Id;
end

go
create procedure Create_usr
	@Name varchar(50),
	@Lastname varchar(50),
	@email varchar(100),
	@password varchar(max)
as
begin transaction TX_Create
	begin try
		INSERT INTO Users(Name, Lastname, email, password) values (@Name, @Lastname, @email,@password);
		commit TX_Create
		SELECT 'Usuario Creado correctamente' as Respuesta, 0 as Error
	end try
	begin catch
		rollback TX_Create
		SELECT ERROR_MESSAGE() as Respuesta, 1 as Error
	end catch

	-- Sin return ----
go
create procedure Create_usr
	@Name varchar(50),
	@Lastname varchar(50),
	@email varchar(100),
	@password varchar(max)
as
begin transaction TX_Create
	begin try
		INSERT INTO Users(Name, Lastname, email, password) values (@Name, @Lastname, @email,@password);
		commit transaction TX_Create
	end try
	begin catch
		rollback transaction TX_Create
	end catch

go
create procedure Delete_usr
	@Id int
as
begin transaction TX_Delete
	BEGIN TRY
		DELETE  FROM Users where Id = @Id
		commit transaction TX_Delete
		SELECT 'Usuario eliminado Correctamente' as Respuesta, 0 as Error
	END TRY
	BEGIN CATCH
		rollback transaction TX_Delete
		SELECT ERROR_MESSAGE() as Respuesta, 1 as Error
	END CATCH

go
create procedure Update_usr	
	@Id int,
	@Name varchar(50),
	@Lastname varchar(50),
	@email varchar(100),
	@password varchar(max)
as
begin transaction TX_Update
	BEGIN TRY
		UPDATE Users set Name = ISNULL(@Name, Name), Lastname =ISNULL( @Lastname, Lastname), email =ISNULL( @email, email), password = ISNULL(@password, password) where Id = @Id
		commit transaction TX_Update
		SELECT 'Usuario actualizado correctamente' as Respuesta, 0 as Error
	END TRY
	BEGIN CATCH
		rollback transaction TX_Update
		SELECT ERROR_MESSAGE() as Respuesta, 1 as Error
	END CATCH
	

----------------------------------------------
create table PRODUCTO(
	IdProducto int primary key identity,
	CodigoBarra varchar(50) unique,
	Nombre varchar(50),
	Marca varchar(50),
	Categoria varchar(100),
	Precio decimal(10,2)
);

INSERT INTO PRODUCTO(CodigoBarra,Nombre,Marca,Categoria,Precio) values
('50910010','Monitor Aoc - Curvo Gaming ','AOC','Tecnologia','1200'),
('50910011','IdeaPad 3i','LENOVO','Tecnologia','1700'),
('50910012','SoyMomo Tablet Lite','SOYMOMO','Tecnologia','300'),
('50910013','Lavadora 21 KG WLA-21','WINIA','ElectroHogar','1749'),
('50910014','Congelador 145 Lt Blanco','ELECTROLUX','ElectroHogar','779'),
('50910015','Cafetera TH-130','THOMAS','ElectroHogar','119'),
('50910016','Reloj análogo Hombre 058','GUESS','Accesorios','699'),
('50910017','Billetera de Cuero Mujer Sophie','REYES','Accesorios','270'),
('50910018','Bufanda Rec Mango Mujer','MANGO','Accesorios','169.90'),
('50910019','Sofá Continental 3 Cuerpos','MICA','Muebles','1299'),
('50910020','Futón New Elina 3 Cuerpos','MICA','Muebles','1349'),
('50910021','Mesa Comedor Volterra 6 Sillas','TUHOME','Muebles','624.12');


select * from PRODUCTO

go
create procedure sp_lista_productos
	as
	begin
	select
	IdProducto,CodigoBarra,Nombre,
	Marca,Categoria,Precio
	from PRODUCTO
end
go


go
create procedure sp_guardar_producto(
	@codigoBarra varchar(50),
	@nombre varchar(50),
	@marca varchar(50),
	@categoria varchar(100),
	@precio decimal(10,2)
)as
begin transaction tx_guardar_producto
	begin try
		insert into PRODUCTO(CodigoBarra,Nombre,Marca,Categoria,Precio)values(@codigoBarra,@nombre,@marca,@categoria,@precio)
		commit transaction tx_guardar_producto
	end try
	begin catch
		rollback transaction tx_guardar_producto
	end catch

go
create proc sp_editar_producto(
	@idProducto int,
	@codigoBarra varchar(50) null,
	@nombre varchar(50) null,
	@marca varchar(50) null,
	@categoria varchar(100) null,
	@precio decimal(10,2) null
)as
begin transaction tx_editar_producto
	begin try
		update PRODUCTO set
		CodigoBarra = isnull(@codigoBarra,CodigoBarra),
		Nombre = isnull(@nombre,Nombre),
		Marca = isnull(@marca,Marca),
		Categoria = isnull(@categoria,Categoria),
		Precio = ISNULL(@precio,Precio)
		where IdProducto = @idProducto;
		commit transaction tx_editar_producto
	end try
	begin catch
		rollback transaction tx_editar_producto
	end catch

go
create proc sp_eliminar_producto(
	@idProducto int
)as
begin transaction tx_eliminar_producto
	begin try
		delete from PRODUCTO where IdProducto = @idProducto
		commit transaction tx_eliminar_producto
	end try
	begin catch
		rollback transaction tx_eliminar_producto
	end catch
