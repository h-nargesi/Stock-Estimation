select PreviousID, COUNT(*) as Counting
from Trade
group by PreviousID
having COUNT(*) > 1
order by Counting desc
