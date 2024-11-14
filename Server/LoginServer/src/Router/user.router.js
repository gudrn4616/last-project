import express from 'express';

const router = express.Router();

router.post('/sign-up', async (req, res) => {
  const { email, password } = req.body;
});

export default router;
