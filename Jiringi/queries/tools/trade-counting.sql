declare @Minsize int = 265,
        @Count int = 10;

select * from ActiveInstuments(@Minsize, @Count)