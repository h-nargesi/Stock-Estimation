SELECT t.PreviousID, SUM(case when d.ID is not null then 1 else 0 end) as Counting
FROM Trade t left join Trade d on t.PreviousID = d.ID
WHERE t.PreviousID IS NOT NULL
GROUP BY t.PreviousID
HAVING SUM(case when d.ID is not null then 1 else 0 end) != 1
ORDER BY Counting desc