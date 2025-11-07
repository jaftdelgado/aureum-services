import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule } from '@nestjs/config';
import { Asset } from './entities/asset.entity';
import { CustomAsset } from './entities/customAsset.entity';
import { AssetCategory } from './entities/assetCategory.entity';
import { AssetController } from './controller/asset.controller';
import { AssetService } from './service/asset.service';

@Module({
  imports: [
    ConfigModule.forRoot(),
    TypeOrmModule.forRoot({
      type: 'postgres',
      url: process.env.DATABASE_URL,
      entities: [Asset, CustomAsset, AssetCategory],
      synchronize: false,
      ssl: { rejectUnauthorized: false },
    }),
    TypeOrmModule.forFeature([Asset, CustomAsset, AssetCategory]),
  ],
  controllers: [AssetController],
  providers: [AssetService],
})
export class AppModule {}
