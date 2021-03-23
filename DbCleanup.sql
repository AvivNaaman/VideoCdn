
use [VideoCdn]

-- basically, if it's null then it has no encoded data, if it has no encoded data then we don't stream it,
-- then it's not part of our catalog!
DELETE FROM [Catalog] WHERE [AvailableResolutions] IS NULL;


