import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { DatabaseModule } from '@modules/database.module';
import { AssetsModule } from '@modules/assets.module';
import { MarketModule } from '@modules/market.module';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
    }),

    DatabaseModule,
    AssetsModule,
    MarketModule,
  ],
})
export class AppModule {}
