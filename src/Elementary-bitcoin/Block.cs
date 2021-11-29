﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LBitcoin {
    class Block {

        protected uint version_;
        protected byte[] prevBlock_;
        protected byte[] merkleRoot_;
        protected int timestamp_;
        protected byte[] bits_;
        protected byte[] nonce_;
        protected List<byte[]> hashes_;

        private BigInteger target_;
        private List<Transaction> txs_;

        public Block(uint version, byte[] prevBlock, byte[] merkleRoot, int timestamp, 
            byte[] bits, byte[] nonce, List<byte[]> hashes = null) {
            version_ = version;
            prevBlock_ = prevBlock;
            merkleRoot_ = merkleRoot;
            timestamp_ = timestamp;
            bits_ = bits;
            nonce_ = nonce;
            hashes_ = hashes;
        }

        public Block(uint version, byte[] prevBlock, byte[] merkleRoot, int timestamp,
            byte[] bits, byte[] nonce, List<Transaction> txs) 
                : this(version, prevBlock, merkleRoot, timestamp, bits, nonce) {

            txs_ = txs;
            foreach(Transaction tx in txs) {
                hashes_.Add(tx.getHash());
            }
        }

        public uint Version { get { return version_; } }

        public byte[] PrevBlock { get { return prevBlock_; } }

        public byte[] MerkleRoot { get { return merkleRoot_; } }

        public int Timestamp { get { return timestamp_; } }

        public byte[] Bits { get { return bits_; } }

        public byte[] Nonce { get { return nonce_; } }

        public List<byte[]> TxHashes { get { return hashes_; } }

        public BigInteger Target { get { return timestamp_; } }

        public List<Transaction> Transactions { get { return txs_; } }

        static public Block Parse(Stream s) {
            byte[] versionBytes = new byte[4];
            s.Read(versionBytes, 0, 4);
            uint version = BitConverter.ToUInt32(versionBytes);
            byte[] prevBlock = new byte[32];
            s.Read(prevBlock, 0, 32);
            byte[] merkleRoot = new byte[32];
            s.Read(merkleRoot, 0, 32);
            byte[] timestampBytes = new byte[4];
            s.Read(timestampBytes, 0, 4);
            int timestamp = BitConverter.ToInt32(timestampBytes);
            byte[] bits = new byte[4];
            s.Read(bits, 0, 4);
            byte[] nonce = new byte[4];
            s.Read(nonce, 0, 4);
            Block block = new Block(version, prevBlock, merkleRoot, timestamp, bits, nonce);
            return block;
        }

        public byte[] Serialise() {
            byte[] versionBytes = BitConverter.GetBytes(version_);
            byte[] tmp = Byte.join(versionBytes, prevBlock_);
            tmp = Byte.join(tmp, merkleRoot_);
            tmp = Byte.join(tmp, BitConverter.GetBytes(timestamp_));
            tmp = Byte.join(tmp, bits_);
            tmp = Byte.join(tmp, nonce_);
            return tmp;
        }

        public byte[] getHeader() {
            byte[] result = Byte.join(BitConverter.GetBytes(version_), prevBlock_);
            result = Byte.join(result, merkleRoot_);
            result = Byte.join(result, BitConverter.GetBytes(timestamp_));
            result = Byte.join(result, bits_);
            result = Byte.join(result, nonce_);
            return result;
        }

        public void fillTransactions(List<Transaction> txs) {
            if(txs.Count < 1) {
                throw new Exception("Number of transactions insufficient");
            } 
            txs_ = txs;
        }

        public void addTransaction(Transaction tx) {
            txs_.Add(tx);
        }

        public byte[] hash() {
            byte[] bytes = this.Serialise();
            return Hash.hash256(bytes);
        }

        public bool bip9() {
            return version_ >> 29 == 1;
        }

        public bool bip91() {
            return (version_ >> 4 & 1) == 1;
        }

        public bool bip141() {
            return ((version_ >> 1) & 1) == 1;
        }

        public double difficulty() {

            return 0xffff * BigIntExtensions.DivideAndReturnDouble(BigInteger.Pow(256, 0x1d - 3) , target_);
        }

        public BigInteger target() {
            return Helper.bitsToTarget(bits_);
        }

        public bool checkPOW() {
            byte[] sha = Hash.hash256(this.Serialise());
            BigInteger proof = new BigInteger(sha, true);
            return proof < target_;
        }
    }
}
