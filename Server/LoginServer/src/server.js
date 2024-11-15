import express from 'express';
import bcrypt from 'bcrypt';
import { createUser, findUserByEmail } from '../db/user/user.db.js';
import { redisManager } from '../redis/redis.init.js';

const saltRounds = 10;
const app = express();
app.use(bodyParser.json());

app.post('/login', async (req, res) => {
  const { email, password } = req.body;

  const user = findUserByEmail(email);

  if (!user) {
    res.status(401).json({ message: '존재하지 않은 이메일입니다.' });
  }
  //암호키와 비밀번호 일치하는지 확인 로직 구현
  const result = await bcrypt.compare(password, user.password);
  if (!result) {
    res.status(401).json({ message: '비밀번호가 틀렸습니다.' });
  }

  const doubleLogin = redisManager.getCache(user.nickname);
  if (doubleLogin) {
    res.status(409).json({ message: '현재 접속 중입니다.' });
  }

  return res.status(201).json('////'); //db에서 조회한 데이터의 userid와 ninkname을 보내면됨
});

//아이디,비밀번호,
app.post('/register', async (req, res) => {
  const { email, password, nickname } = req.body;

  const hashedPassword = await bcrypt.hash(password, saltRounds);

  //db에 유저 정보 추가

  return res.status(201).json({ message: '회원가입 완료' });
});

app.listen(5000, () => {
  console.log(`LoginServer is running on port 5000`);
});
